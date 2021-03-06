/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;
using System.CodeDom;

using IronPython.Runtime;
using IronPython.CodeDom;

namespace IronPython.Compiler {
    [FlagsAttribute]
    public enum FuncDefType { None = 0, ArgList, KeywordDict }

    /// <summary>
    /// Summary description for FuncDef.
    /// </summary>
    public partial class FuncDef : ScopeStatement {
        private static int counter = 0;

        const string tupleArgHeader = "tupleArg#";

        public Location header;
        public readonly Name name;
        public readonly Expr[] parameters;
        public readonly Expr[] defaults;
        public readonly FuncDefType flags;
        public Expr decorators;
        public string filename;
        public int yieldCount = 0;

        public FuncDef(Name name, Expr[] parameters, Expr[] defaults, FuncDefType flags, string sourceFile)
            : this(name, parameters, defaults, flags, null, sourceFile) {
        }

        public FuncDef(Name name, Expr[] parameters, Expr[] defaults, FuncDefType flags, Stmt body, string sourceFile)
            : base(body) {
            this.name = name;
            this.parameters = parameters;
            this.defaults = defaults;
            this.flags = flags;
            this.decorators = null;
            this.filename = sourceFile;
        }

        public object MakeFunction(NameEnv env) {
            string[] names = Name.ToStrings(makeNames(parameters));
            object[] defaults = Expr.Evaluate(this.defaults, env);
            return new InterpFunction(name.GetString(), names, defaults, body, env.globals);
        }

        public override object Execute(NameEnv env) {
            env.Set(name.GetString(), MakeFunction(env));
            return NextStmt;
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in parameters) e.Walk(w);
                foreach (Expr e in defaults) e.Walk(w);
                body.Walk(w);
            }
            w.PostWalk(this);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitPosition(start, header);
            SignatureInfo sigInfo = SignatureInfo.Get(this, cg);

            FlowChecker.Check(this);

            string mname = name.GetString() + "$f" + counter++;

            // create the new method & setup it's locals
            CodeGen impl = cg.DefineMethod(mname, typeof(object), sigInfo.ParamTypes, sigInfo.ParamNames);
            impl.Names = CodeGen.CreateLocalNamespace(impl);
            impl.Context = cg.Context;

            for (int arg = sigInfo.HasContext ? 1 : 0; arg < sigInfo.ParamNames.Length; arg++) {
                impl.Names.SetSlot(sigInfo.ParamNames[arg], impl.GetArgumentSlot(arg));
            }

            // then generate the actual method
            EmitFunctionImplementation(cg, impl, sigInfo.HasContext);
            if (NeedsWrapperMethod()) impl = MakeWrapperMethodN(cg, impl.MethodInfo, sigInfo.HasContext);

            //  Create instance of the Function? object
            Type funcType, targetType;
            using (impl) {
                GetFunctionType(out funcType, out targetType);
                cg.EmitModuleInstance();
                cg.EmitString(name.GetString());

                cg.EmitDelegate(impl, targetType, sigInfo.ContextSlot);
            }

            int first = sigInfo.HasContext ? 1 : 0;
            //  Emit string array (minus the first environment argument)
            cg.EmitInt(sigInfo.ParamNames.Length - first);
            cg.Emit(OpCodes.Newarr, typeof(string));
            for (int i = first; i < sigInfo.ParamNames.Length; i++) {
                cg.Emit(OpCodes.Dup);
                cg.EmitInt(i - first);
                cg.EmitStringOrNull(sigInfo.ParamNames[i].GetString());
                cg.Emit(OpCodes.Stelem_Ref);
            }
            cg.EmitObjectArray(defaults);

            if (flags == FuncDefType.None) {
                cg.Emit(OpCodes.Newobj, funcType.GetConstructor(
                    new Type[] { typeof(PythonModule), typeof(string), targetType, typeof(string[]), typeof(object[]) }));
            } else {
                cg.EmitInt((int)flags);
                cg.Emit(OpCodes.Newobj, funcType.GetConstructor(
                    new Type[] { typeof(PythonModule), typeof(string), targetType, typeof(string[]), typeof(object[]), typeof(FuncDefType) }));
            }

            string doc = body.GetDocString();
            if (doc != null) {
                cg.Emit(OpCodes.Dup);
                cg.EmitString(doc);
                cg.EmitCall(typeof(PythonFunction).GetProperty("Documentation").GetSetMethod());
            }

            // update func_code w/ appropriate state.
            cg.Emit(OpCodes.Dup);

            cg.EmitCall(typeof(PythonFunction).GetProperty("FunctionCode").GetGetMethod());
            cg.Emit(OpCodes.Castclass, typeof(FunctionCode));
            cg.Emit(OpCodes.Dup);
            cg.EmitInt(this.start.line);
            cg.EmitCall(typeof(FunctionCode), "SetLineNumber");

            cg.EmitString(this.filename);
            cg.EmitCall(typeof(FunctionCode), "SetFilename");

            cg.EmitSet(name);

            if (decorators != null) {
                decorators.Emit(cg);
                cg.EmitSet(name);
            }
        }


        class SignatureInfo {
            public static SignatureInfo Get(FuncDef fd, CodeGen cg) {
                int first = 0;
                Type[] paramTypes;
                Name[] paramNames;
                Slot contextSlot = null;

                if (fd.IsClosure) {
                    contextSlot = cg.EnvironmentSlot;
                } else {
                    contextSlot = cg.ContextSlot;
                }

                if (contextSlot != null) {
                    first = 1;          // Skip the first argument 
                    paramTypes = new Type[fd.parameters.Length + 1];
                    paramNames = new Name[fd.parameters.Length + 1];

                    paramTypes[0] = contextSlot.Type;
                    paramNames[0] = Name.Make("$env");

                    for (int i = 1; i < paramTypes.Length; i++) {
                        paramTypes[i] = typeof(object);
                        paramNames[i] = makeName(fd.parameters[i - 1]);
                    }
                } else {
                    paramTypes = CompilerHelpers.MakeRepeatedArray(typeof(object), fd.parameters.Length);
                    paramNames = makeNames(fd.parameters);
                }

                return new SignatureInfo(paramTypes, paramNames, first > 0, contextSlot);
            }

            private SignatureInfo(Type[] paramTypes, Name[] paramNames, bool hasContext, Slot contextSlot) {
                ParamTypes = paramTypes;
                ParamNames = paramNames;
                HasContext = hasContext;
                ContextSlot = contextSlot;
            }

            public readonly Type[] ParamTypes;
            public readonly Name[] ParamNames;
            public readonly bool HasContext;
            public readonly Slot ContextSlot;
        }

        private void EmitFunctionImplementation(CodeGen cg, CodeGen icg, bool context) {
            if (context) {
                if (IsClosure) {
                    icg.StaticLinkSlot = icg.GetArgumentSlot(0);
                }
                icg.ContextSlot = icg.GetArgumentSlot(0);
                icg.ModuleSlot = new PropertySlot(icg.ContextSlot, typeof(ICallerContext).GetProperty("Module"));
            }

            if (EmitLocalDictionary) {
                PromoteLocalsToEnvironment();
            }

            if (Options.TracebackSupport) {
                // push a try for traceback support
                icg.PushTryBlock();
                icg.BeginExceptionBlock();
            }

            // emit the actual body
            if (yieldCount > 0) {
                EmitGeneratorBody(icg, cg);
            } else {
                EmitFunctionBody(icg, cg);
            }

            if (Options.TracebackSupport) {
                // push a fault block (runs only if there's an exception, doesn't handle the exception)
                icg.PopTargets();
                if (icg.IsDynamicMethod) {
                    icg.BeginCatchBlock(typeof(Exception));
                } else {
                    icg.BeginFaultBlock();
                }

                EmitUpdateTraceBack(icg, cg);

                // end the exception block
                if (icg.IsDynamicMethod) {
                    icg.Emit(OpCodes.Rethrow);
                }
                icg.EndExceptionBlock();
            }

            icg.Finish();
        }

        private void EmitUpdateTraceBack(CodeGen cg, CodeGen ocg) {
            cg.EmitCallerContext();
            cg.EmitString(name.GetString());
            cg.EmitString(filename);
            cg.EmitGetCurrentLine();
            cg.EmitCall(typeof(Ops), "UpdateTraceBack");
        }

        private bool NeedsWrapperMethod() {
            return parameters.Length > Ops.MaximumCallArgs || flags != FuncDefType.None;
        }

        private CodeGen MakeWrapperMethodN(CodeGen cg, MethodInfo impl, bool context) {
            CodeGen icg;
            int index = 0;
            if (context) {
                Type environmentType = IsClosure ? cg.EnvironmentSlot.Type : cg.ContextSlot.Type;
                icg = cg.DefineUserHiddenMethod(impl.Name, typeof(object), new Type[] { environmentType, typeof(object[]) });
                Slot env = icg.GetArgumentSlot(index++);
                env.EmitGet(icg);
            } else {
                icg = cg.DefineUserHiddenMethod(impl.Name, typeof(object), new Type[] { typeof(object[]) });
            }

            Slot arg = icg.GetArgumentSlot(index);

            for (int pi = 0; pi < parameters.Length; pi++) {
                arg.EmitGet(icg);
                icg.EmitInt(pi);
                icg.Emit(OpCodes.Ldelem_Ref);
            }
            icg.EmitCall(impl);
            icg.Emit(OpCodes.Ret);
            return icg;
        }

        private void EmitTupleParams(CodeGen cg) {
            for (int i = 0; i < parameters.Length; i++) {
                Expr p = parameters[i];
                if (p is NameExpr) continue;

                //!!! not too clean
                cg.Names[EncodeTupleParamName(p as TupleExpr)].EmitGet(cg);

                p.EmitSet(cg);
            }
        }

        private void EmitFunctionBody(CodeGen cg, CodeGen ocg) {
            if (HasEnvironment) {
                cg.ContextSlot = cg.EnvironmentSlot = CreateEnvironment(cg);
            }
            if (cg.ContextSlot == null && IsClosure) {
                cg.ContextSlot = cg.StaticLinkSlot;
            }

            // Populate the environment with slots
            CreateGlobalSlots(cg, ocg);
            CreateClosureSlots(cg);
            CreateLocalSlots(cg);
            EmitTupleParams(cg);
            body.Emit(cg);
            cg.EmitReturn(null);
        }

        private void CreateGeneratorTemps(EnvironmentFactory ef, CodeGen cg) {
            for (int i = 0; i < tempsCount; i++) {
                cg.Names.AddTempSlot(ef.MakeEnvironmentReference(Name.Make("temp$" + i)).CreateSlot(cg.EnvironmentSlot));
            }
        }

        private void EmitGeneratorBody(CodeGen cg, CodeGen ocg) {
            // Create the GenerateNext function
            CodeGen ncg = cg.DefineMethod(name.GetString() + "$g" + counter++, typeof(bool),
                new Type[] { typeof(Generator), typeof(object).MakeByRefType() },
                new String[] { "$gen", "$ret" });

            PromoteLocalsToEnvironment();

            // Namespace without er factory - all locals must exist ahead of time
            ncg.Names = new Namespace(null);
            Slot generator = ncg.GetArgumentSlot(0);
            ncg.StaticLinkSlot = new FieldSlot(generator, typeof(Generator).GetField("staticLink"));
            if (HasEnvironment) {
                cg.EnvironmentSlot = CreateEnvironment(cg);
                EnvironmentFactory ef = this.environmentFactory;
                Slot envSlotCast = new CastSlot(
                    new FieldSlot(generator, typeof(Generator).GetField("environment")),
                    ef.EnvironmentType
                    );
                Slot envSlot = ncg.GetLocalTmp(ef.EnvironmentType);
                // setup the environment and static link slots
                ncg.EnvironmentSlot = envSlot;
                ncg.ContextSlot = envSlot;
                // pull the environment into typed local variable
                envSlot.EmitSet(ncg, envSlotCast);
                InheritEnvironment(ncg);
                CreateGeneratorTemps(ef, ncg);
            } else {
                ncg.ContextSlot = ncg.StaticLinkSlot;
            }
            ncg.ModuleSlot = new PropertySlot(ncg.ContextSlot, typeof(ICallerContext).GetProperty("Module"));

            CreateClosureSlots(ncg);
            CreateGlobalSlots(ncg, ocg);

            // Emit the generator body using the typed er
            EmitGenerator(ncg);

            // Initialize the generator
            EmitTupleParams(cg);

            // Create instance of the generator
            cg.EmitStaticLinkOrNull();
            cg.EmitEnvironmentOrNull();
            cg.EmitDelegate(ncg, typeof(Generator.NextTarget), null);
            cg.EmitNew(typeof(Generator), new Type[] { typeof(FunctionEnvironmentDictionary), typeof(FunctionEnvironmentDictionary), typeof(Generator.NextTarget) });
            cg.EmitReturn();
        }

        private void InheritEnvironment(CodeGen cg) {
            if (environment == null) return;
            foreach (KeyValuePair<Name, EnvironmentReference> kv in environment) {
                Slot slot = kv.Value.CreateSlot(cg.EnvironmentSlot);
                cg.Names[kv.Key] = slot;
            }
        }

        private void EmitGenerator(CodeGen ncg) {
            YieldTarget[] targets = YieldLabelBuilder.BuildYieldTargets(this, ncg);

            Label[] jumpTable = new Label[yieldCount];
            for (int i = 0; i < yieldCount; i++) jumpTable[i] = targets[i].topBranchTarget;
            ncg.yieldLabels = jumpTable;

            ncg.PushTryBlock();
            ncg.BeginExceptionBlock();

            ncg.Emit(OpCodes.Ldarg_0);
            ncg.EmitFieldGet(typeof(Generator), "location");
            ncg.Emit(OpCodes.Switch, jumpTable);

            // fall-through on first pass
            // yield statements will insert the needed labels after their returns
            body.Emit(ncg);

            // fall-through is almost always possible in generators, so this
            // is almost always needed
            ncg.EmitReturnInGenerator(null);

            // special handling for StopIteration thrown in body
            ncg.BeginCatchBlock(typeof(StopIterationException));
            ncg.EndExceptionBlock();
            ncg.EmitReturnInGenerator(null);
            ncg.PopTargets();

            ncg.Finish();
        }

        public override bool TryGetBinding(Name name, out Binding binding) {
            if (names.TryGetValue(name, out binding)) {
                return binding.IsBound;
            } else return false;
        }

        private static Name EncodeTupleParamName(TupleExpr param) {
            // we encode a tuple parameter so we can extract the compound
            // members back out of it's name.
            StringBuilder sb = new StringBuilder(tupleArgHeader);
            AppendTupleParamNames(sb, param);

            return Name.Make(sb.ToString());
        }

        private static void AppendTupleParamNames(StringBuilder sb, TupleExpr param) {
            for (int i = 0; i < param.items.Length; i++) {
                NameExpr ne = param.items[i] as NameExpr;
                if (ne != null) {
                    sb.Append('!');
                    sb.Append(ne.name.GetString());
                } else {
                    // nested tuple
                    AppendTupleParamNames(sb, param.items[i] as TupleExpr);
                }
            }
        }

        /// <summary>
        /// Returns a tuple of argument names.  Nested tuple args are
        /// represented as Tuple's nested in the array.
        /// </summary>
        internal static Tuple DecodeTupleParamName(string name) {
            // encoding is: tupleArg#!argName[!argName ...]
            // nestings can occur in which case we get:
            // tupleArg#!argName!tupleArg#!(encoding arg names)#

            Debug.Assert(String.Compare(name, 0, tupleArgHeader, 0, tupleArgHeader.Length) == 0);

            int curIndex = name.IndexOf('!');
            List<string> names = new List<string>();
            while (curIndex != -1) {
                Debug.Assert(curIndex != (name.Length - 1));

                int nextindex = name.IndexOf('!', curIndex + 1);
                if (nextindex == -1) {
                    names.Add(name.Substring(curIndex + 1));
                    break;
                }
                names.Add(name.Substring(curIndex + 1, nextindex - (curIndex + 1)));

                curIndex = nextindex;
            }
            return new Tuple(names);
        }

        private static Name makeName(Expr param) {
            NameExpr ne = param as NameExpr;
            if (ne == null) {
                return EncodeTupleParamName((TupleExpr)param);
            } else {
                return ne.name;
            }
        }

        private static Name[] makeNames(Expr[] parameters) {
            Name[] ret = new Name[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                ret[i] = makeName(parameters[i]);
            }
            return ret;
        }
    }

    public struct YieldTarget {
        public Label topBranchTarget;
        public Label tryBranchTarget;
        public bool finallyBranch;

        public YieldTarget(Label topBranchTarget) {
            this.topBranchTarget = topBranchTarget;
            tryBranchTarget = new Label();
            finallyBranch = false;
        }

        public YieldTarget FixForTry(CodeGen cg) {
            tryBranchTarget = cg.DefineLabel();
            return this;
        }

        public YieldTarget FixForFinally(CodeGen cg) {
            tryBranchTarget = cg.DefineLabel();
            finallyBranch = true;
            return this;
        }
    }

    class YieldLabelBuilder : AstWalker {
        public abstract class ExceptionBlock {
            public enum State {
                Try,
                Handler,
                Finally
            };
            public State state;

            protected ExceptionBlock(State state) {
                this.state = state;
            }

            public abstract void AddYieldTarget(YieldStmt ys, YieldTarget yt, CodeGen cg);
        }

        public sealed class TryBlock : ExceptionBlock {
            private TryStmt stmt;

            public TryBlock(TryStmt stmt)
                : this(stmt, State.Try) {
            }
            public TryBlock(TryStmt stmt, State state)
                : base(state) {
                this.stmt = stmt;
            }

            public override void AddYieldTarget(YieldStmt ys, YieldTarget yt, CodeGen cg) {
                switch (state) {
                    case State.Try:
                        stmt.AddYieldTarget(yt.FixForTry(cg));
                        ys.label = yt.tryBranchTarget;
                        break;
                    case State.Handler:
                        stmt.yieldInExcept = true;
                        ys.label = yt.topBranchTarget;
                        break;
                }
            }
        }

        public sealed class TryFinallyBlock : ExceptionBlock {
            private TryFinallyStmt stmt;

            public TryFinallyBlock(TryFinallyStmt stmt)
                : this(stmt, State.Try) {
            }
            public TryFinallyBlock(TryFinallyStmt stmt, State state)
                : base(state) {
                this.stmt = stmt;
            }

            public override void AddYieldTarget(YieldStmt ys, YieldTarget yt, CodeGen cg) {
                switch (state) {
                    case State.Try:
                        cg.Context.AddError("cannot yield from try block with finally", ys);
                        break;
                    case State.Finally:
                        stmt.AddYieldTarget(yt.FixForFinally(cg));
                        ys.label = yt.tryBranchTarget;
                        break;
                }
            }
        }

        Stack<ExceptionBlock> tryBlocks = new Stack<ExceptionBlock>();
        YieldTarget[] topYields;
        FuncDef func;
        CodeGen cg;

        private YieldLabelBuilder(FuncDef func, CodeGen cg) {
            this.func = func;
            this.cg = cg;
            this.topYields = new YieldTarget[func.yieldCount];
        }

        public static YieldTarget[] BuildYieldTargets(FuncDef func, CodeGen cg) {
            YieldLabelBuilder b = new YieldLabelBuilder(func, cg);
            func.Walk(b);
            return b.topYields;
        }

        #region AstWalker method overloads

        public override bool Walk(FuncDef node) {
            // Do not recurse into nested functions
            return node == func;
        }

        public override bool Walk(TryFinallyStmt node) {
            TryFinallyBlock tfb = new TryFinallyBlock(node);
            tryBlocks.Push(tfb);
            node.body.Walk(this);
            tfb.state = ExceptionBlock.State.Finally;
            node.finallyStmt.Walk(this);
            ExceptionBlock eb = tryBlocks.Pop();
            Debug.Assert((object)eb == (object)tfb);
            return false;
        }


        public override bool Walk(TryStmt node) {
            TryBlock tb = new TryBlock(node);
            tryBlocks.Push(tb);
            node.body.Walk(this);

            tb.state = TryBlock.State.Handler;
            foreach (TryStmtHandler handler in node.handlers) {
                handler.Walk(this);
            }

            ExceptionBlock eb = tryBlocks.Pop();
            Debug.Assert((object)tb == (object)eb);

            if (node.elseStmt != null) {
                node.elseStmt.Walk(this);
            }
            return false;
        }

        public override void PostWalk(YieldStmt node) {
            topYields[node.index] = new YieldTarget(cg.DefineLabel());

            if (tryBlocks.Count == 0) {
                node.label = topYields[node.index].topBranchTarget;
            } else if (tryBlocks.Count == 1) {
                ExceptionBlock eb = tryBlocks.Peek();
                eb.AddYieldTarget(node, topYields[node.index], cg);
            } else {
                throw new NotImplementedException("yield in more than one try block");
            }
        }

        #endregion
    }

}
