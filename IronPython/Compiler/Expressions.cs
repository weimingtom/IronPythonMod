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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom;
using System.Text;
using System.Diagnostics;

using IronPython.Runtime;
using IronPython.CodeDom;

namespace IronPython.Compiler {
    /// <summary>
    /// Summary description for Expr.
    /// </summary>
    public abstract class Expr : Node {
        public virtual object Evaluate(NameEnv env) {
            throw new NotImplementedException("Evaluate: " + this);
        }

        public virtual void Assign(object val, NameEnv env) {
            throw new NotImplementedException("Assign: " + this);
        }

        public abstract void Emit(CodeGen cg);

        public virtual void EmitSet(CodeGen cg) {
            cg.Context.AddError("can't assign to " + this.GetType().Name, this);
        }

        public virtual void EmitDel(CodeGen cg) {
            throw new NotImplementedException("EmitDel: " + this);
        }

        public static object[] Evaluate(Expr[] items, NameEnv env) {
            object[] ret = new object[items.Length];
            for (int i = 0; i < items.Length; i++) {
                ret[i] = items[i].Evaluate(env);
            }
            return ret;
        }
    }

    public class ErrorExpr : Expr {
        public override void Walk(IAstWalker w) {
        }

        public override void Emit(CodeGen cg) {
            throw new NotImplementedException("ErrorExpr.Emit");
        }
    }

    public class CallExpr : Expr {
        public static readonly Name ParamsName = Name.Make("*");
        public static readonly Name DictionaryName = Name.Make("**");
        private static readonly Name LocalsName = Name.Make("locals");
        private static readonly Name VarsName = Name.Make("vars");
        private static readonly Name DirName = Name.Make("dir");
        private static readonly Name EvalName = Name.Make("eval");

        public readonly Expr target;
        public readonly Arg[] args;
        private bool hasArgsTuple, hasKeywordDict;
        private int keywordCount, extraArgs;

        public CallExpr(Expr target, Arg[] args, bool hasArgsTuple, bool hasKeywordDict, int keywordCount, int extraArgs) {
            this.target = target;
            this.args = args;
            this.hasArgsTuple = hasArgsTuple;
            this.hasKeywordDict = hasKeywordDict;
            this.keywordCount = keywordCount;
            this.extraArgs = extraArgs;
        }

        public bool MightNeedLocalsDictionary() {
            NameExpr nameExpr = target as NameExpr;
            if (nameExpr == null) return false;

            if (args.Length == 0) {
                if (nameExpr.name == LocalsName) return true;
                if (nameExpr.name == VarsName) return true;
                if (nameExpr.name == DirName) return true;
                return false;
            } else if (args.Length == 1) {
                if (nameExpr.name == EvalName) return true;
            }
            return false;
        }

        public override void EmitDel(CodeGen cg) {
            cg.Context.AddError("can't delete function call", this);
        }

        public override object Evaluate(NameEnv env) {
            object callee = target.Evaluate(env);

            object[] cargs = new object[args.Length];
            int index = 0;
            foreach (Arg arg in args) {
                if (arg.name != null) throw new NotImplementedException("keywords");
                cargs[index++] = arg.expr.Evaluate(env);
            }

            switch (cargs.Length) {
                case 0: return Ops.Call(callee);
                default: return Ops.Call(callee, cargs);
            }
        }

        public override void Emit(CodeGen cg) {
            //!!! first optimize option comes here
            //			if (target is FieldExpr && !hasSpecialArgs()) {
            //				generateInvoke((FieldExpr)target, cg);
            //				return;
            //			}

            Label done = new Label();
            bool emitDone = false;

            Expr[] exprs = new Expr[args.Length - extraArgs];
            Expr argsTuple = null, keywordDict = null;
            string[] keywordNames = new string[keywordCount];
            int index = 0, keywordIndex = 0;
            foreach (Arg arg in args) {
                if (arg.name == ParamsName) {
                    argsTuple = arg.expr; continue;
                } else if (arg.name == DictionaryName) {
                    keywordDict = arg.expr; continue;
                } else if (arg.name != null) {
                    keywordNames[keywordIndex++] = arg.name.GetString();
                }
                exprs[index++] = arg.expr;
            }

            if (hasKeywordDict || (hasArgsTuple && keywordCount > 0)) {
                cg.EmitCallerContext();
                target.Emit(cg);
                cg.EmitObjectArray(exprs);
                cg.EmitStringArray(keywordNames);
                cg.EmitExprOrNone(argsTuple);
                cg.EmitExprOrNone(keywordDict);
                cg.EmitCall(typeof(Ops), "CallWithArgsTupleAndKeywordDictAndContext",
                    new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]), typeof(string[]),
							   typeof(object), typeof(object)});
            } else if (hasArgsTuple) {
                cg.EmitCallerContext();
                target.Emit(cg);
                cg.EmitObjectArray(exprs);
                cg.EmitExprOrNone(argsTuple);
                cg.EmitCall(typeof(Ops), "CallWithArgsTupleAndContext",
                    new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]), typeof(object) });
            } else if (keywordCount > 0) {
                target.Emit(cg);
                cg.EmitObjectArray(exprs);
                cg.EmitStringArray(keywordNames);
                cg.EmitCall(typeof(Ops), "Call",
                    new Type[] { typeof(object), typeof(object[]), typeof(string[]) });
            } else {
                cg.EmitCallerContext();
                target.Emit(cg);
                if (args.Length <= 4) {
                    Type[] argTypes = new Type[args.Length + 2];
                    int i = 0;
                    argTypes[i++] = typeof(ICallerContext);
                    argTypes[i++] = typeof(object);
                    foreach (Expr e in exprs) {
                        e.Emit(cg);
                        argTypes[i++] = typeof(object);
                    }
                    cg.EmitCall(typeof(Ops), "CallWithContext", argTypes);
                } else {
                    cg.EmitObjectArray(exprs);
                    cg.EmitCall(typeof(Ops), "CallWithContext",
                        new Type[] { typeof(ICallerContext), typeof(object), typeof(object[]) });
                }
            }

            if (emitDone) {
                cg.MarkLabel(done);
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                target.Walk(w);
                foreach (Arg arg in args) arg.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class Arg : Node {
        public readonly Name name;
        public readonly Expr expr;
        public Arg(Expr expr) : this(null, expr) { }

        public Arg(Name name, Expr expr) {
            this.name = name;
            this.expr = expr;
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class FieldExpr : Expr {
        public readonly Expr target;
        public readonly Name name;
        public FieldExpr(Expr target, Name name) {
            this.target = target;
            this.name = name;
        }

        public override object Evaluate(NameEnv env) {
            object t = target.Evaluate(env);
            return Ops.GetAttr(env.globals, t, SymbolTable.StringToId(name.GetString()));
        }

        public override void Assign(object val, NameEnv env) {
            object t = target.Evaluate(env);
            Ops.SetAttr(env.globals, t, SymbolTable.StringToId(name.GetString()), val);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitCallerContext();
            target.Emit(cg);

            cg.EmitSymbolId(name.GetString());

            cg.EmitCall(typeof(Ops), "GetAttr"); //, new Type[] { typeof(object), typeof(SymbolId) });
        }

        public override void EmitSet(CodeGen cg) {
            target.Emit(cg);
            cg.EmitSymbolId(name.GetString());
            cg.EmitCallerContext();
            cg.EmitCall(typeof(Ops), "SetAttrStackHelper");
        }

        public override void EmitDel(CodeGen cg) {
            cg.EmitCallerContext();
            target.Emit(cg);
            cg.EmitSymbolId(name.GetString());
            cg.EmitCall(typeof(Ops), "DelAttr");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                target.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class IndexExpr : Expr {
        public readonly Expr target;
        public readonly Expr index;
        public IndexExpr(Expr target, Expr index) {
            this.target = target;
            this.index = index;
        }

        public override object Evaluate(NameEnv env) {
            object t = target.Evaluate(env);
            object i = index.Evaluate(env);
            return Ops.GetIndex(t, i);
        }

        public override void Assign(object val, NameEnv env) {
            object t = target.Evaluate(env);
            object i = index.Evaluate(env);
            Ops.SetIndex(t, i, val);
        }

        public override void Emit(CodeGen cg) {
            target.Emit(cg);
            index.Emit(cg);
            cg.EmitCall(typeof(Ops), "GetIndex");
        }


        public override void EmitSet(CodeGen cg) {
            target.Emit(cg);
            index.Emit(cg);
            cg.EmitCall(typeof(Ops), "SetIndexStackHelper");
        }

        public override void EmitDel(CodeGen cg) {
            target.Emit(cg);
            index.Emit(cg);
            cg.EmitCall(typeof(Ops), "DelIndex");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                target.Walk(w);
                index.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public abstract class SequenceExpr : Expr {
        public readonly Expr[] items;
        protected SequenceExpr(params Expr[] items) { this.items = items; }

        protected abstract string EmptySequenceString { get; }

        public override void Assign(object val, NameEnv env) {
            // Disallow "[] = l", "[], a = l, l", "[[]] = [l]", etc
            if (items.Length == 0) {
                throw Ops.SyntaxError("can't assign to " + EmptySequenceString, "<unknown>", 
                    start.line, start.column, null, 0, IronPython.Hosting.Severity.Error);
            }
            
            IEnumerator ie = Ops.GetEnumerator(val);

            int leftCount = items.Length;
            object[] values = new object[leftCount]; 
            
            int rightCount = Ops.GetEnumeratorValues(ie, ref values);
            if (leftCount != rightCount)
                throw Ops.ValueErrorForUnpackMismatch(leftCount, rightCount);

            for (int i = 0; i < leftCount; i++)
                items[i].Assign(values[i], env);
        }

        public override void EmitSet(CodeGen cg) {
            // Disallow "[] = l", "[], a = l, l", "[[]] = [l]", etc
            if (items.Length == 0) {
                cg.Context.AddError("can't assign to " + EmptySequenceString, this);
                return;
            }

            // int leftCount = items.Length;
            Slot leftCount = cg.GetLocalTmp(typeof(int));
            cg.EmitInt(items.Length);
            leftCount.EmitSet(cg);

            // object[] values = new object[leftCount]; 
            Slot values = cg.GetLocalTmp(typeof(object[]));
            leftCount.EmitGet(cg);
            cg.Emit(OpCodes.Newarr, typeof(object));
            values.EmitSet(cg);

            // ie = Ops.GetEnumerator(<value on stack>)
            Slot ie = cg.GetLocalTmp(typeof(IEnumerator));
            cg.EmitCall(typeof(Ops), "GetEnumerator");
            ie.EmitSet(cg);

            // int rightCount = Ops.GetEnumeratorValues(ie, ref values);
            Slot rightCount = cg.GetLocalTmp(typeof(int));

            ie.EmitGet(cg);
            values.EmitGetAddr(cg);
            cg.EmitCall(typeof(Ops), "GetEnumeratorValues");
            rightCount.EmitSet(cg);

            // if (leftCount != rightCount)
            //      throw Ops.ValueErrorForUnpackMismatch(leftCount, rightCount);
            Label equalSizes = cg.DefineLabel();

            leftCount.EmitGet(cg);
            rightCount.EmitGet(cg);
            cg.Emit(OpCodes.Ceq);
            cg.Emit(OpCodes.Brtrue_S, equalSizes);

            leftCount.EmitGet(cg);
            rightCount.EmitGet(cg);
            cg.EmitCall(typeof(Ops).GetMethod("ValueErrorForUnpackMismatch"));
            cg.Emit(OpCodes.Throw);

            cg.MarkLabel(equalSizes);

            // for (int i = 0; i < leftCount; i++)
            //     items[i].Assign(values[i], env);

            int i = 0;
            foreach (Expr expr in items) {
                values.EmitGet(cg);
                cg.EmitInt(i++);
                cg.Emit(OpCodes.Ldelem_Ref);
                expr.EmitSet(cg);
            }

            cg.FreeLocalTmp(leftCount);
            cg.FreeLocalTmp(rightCount);
            cg.FreeLocalTmp(values);
            cg.FreeLocalTmp(ie);
        }

        public override void EmitDel(CodeGen cg) {
            foreach (Expr expr in items) {
                expr.EmitDel(cg);
            }
        }
    }


    public class TupleExpr : SequenceExpr {
        bool expandable;

        public TupleExpr(params Expr[] items)
            : this(false, items) {
        }
        public TupleExpr(bool expandable, params Expr[] items)
            : base(items) {
            this.expandable = expandable;
        }

        protected override string EmptySequenceString { get { return "()"; } }

        public override object Evaluate(NameEnv env) {
            return Ops.MakeTuple(Evaluate(items, env));
        }

        public override void Emit(CodeGen cg) {
            cg.EmitObjectArray(items);
            cg.EmitCall(typeof(Ops), expandable ? "MakeExpandableTuple" : "MakeTuple", new Type[] { typeof(object[]) });
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in items) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class ListExpr : SequenceExpr {
        public ListExpr(params Expr[] items) : base(items) { }

        protected override string EmptySequenceString { get { return "[]"; } }

        public override object Evaluate(NameEnv env) {
            return Ops.MakeList(Evaluate(items, env));
        }

        public override void Emit(CodeGen cg) {
            cg.EmitObjectArray(items);
            cg.EmitCall(typeof(Ops), "MakeList", new Type[] { typeof(object[]) });
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (Expr e in items) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class DictExpr : Expr {
        public readonly SliceExpr[] items;
        public DictExpr(params SliceExpr[] items) { this.items = items; }

        public override object Evaluate(NameEnv env) {
            IDictionary<object, object> dict = Ops.MakeDict(items.Length);
            foreach (SliceExpr s in items) {
                dict[s.slStart.Evaluate(env)] = s.slStop.Evaluate(env);
            }
            return dict;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitInt(items.Length);
            cg.EmitCall(typeof(Ops), "MakeDict");
            foreach (SliceExpr s in items) {
                cg.Emit(OpCodes.Dup);
                s.slStart.Emit(cg);
                s.slStop.Emit(cg);
                cg.EmitCall(typeof(Dict).GetProperty("Item").GetSetMethod());
            }
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (SliceExpr e in items) e.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class SliceExpr : Expr {
        //!!! these names will be trouble
        public readonly Expr slStart, slStop, slStep;
        public SliceExpr(Expr start, Expr stop, Expr step) {
            this.slStart = start;
            this.slStop = stop;
            this.slStep = step;
        }

        public override object Evaluate(NameEnv env) {
            object e1 = slStart.Evaluate(env);
            object e2 = slStop.Evaluate(env);
            object e3 = slStep.Evaluate(env);
            return Ops.MakeSlice(e1, e2, e3);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitExprOrNone(slStart);
            cg.EmitExprOrNone(slStop);
            cg.EmitExprOrNone(slStep);
            cg.EmitCall(typeof(Ops), "MakeSlice");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                if (slStart != null) slStart.Walk(w);
                if (slStop != null) slStop.Walk(w);
                if (slStep != null) slStep.Walk(w);
            }
            w.PostWalk(this);
        }
    }


    public class BackquoteExpr : Expr {
        public readonly Expr expr;
        public BackquoteExpr(Expr expr) { this.expr = expr; }

        public override object Evaluate(NameEnv env) {
            return Ops.Repr(expr.Evaluate(env));
        }

        public override void Emit(CodeGen cg) {
            expr.Emit(cg);
            cg.EmitCall(typeof(Ops), "Repr");
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }
    public class ParenExpr : Expr {
        public readonly Expr expr;
        public ParenExpr(Expr expr) { this.expr = expr; }

        public override object Evaluate(NameEnv env) {
            return expr.Evaluate(env);
        }

        public override void Emit(CodeGen cg) {
            expr.Emit(cg);
        }

        public override void EmitDel(CodeGen cg) {
            expr.EmitDel(cg);
        }

        public override void EmitSet(CodeGen cg) {
            expr.EmitSet(cg);
        }

        public override void Assign(object val, NameEnv env) {
            expr.Assign(val, env);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }


    public class ConstantExpr : Expr {
        public readonly object value;
        public ConstantExpr(object value) {
            this.value = value;
        }

        public override object Evaluate(NameEnv env) {
            return value;
        }

        public override void Emit(CodeGen cg) {
            cg.EmitConstant(value);
        }

        public override void EmitSet(CodeGen cg) {
            if (value == null) {
                cg.Context.AddError("assignment to None", this);
            }

            cg.Context.AddError("can't assign to literal", this);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }
    }

    public class NameExpr : Expr {
        public readonly Name name;
        public bool defined;

        public NameExpr(Name name) { this.name = name; }

        public override object Evaluate(NameEnv env) {
            return env.Get(name.GetString());
        }

        public override void Assign(object val, NameEnv env) {
            env.Set(name.GetString(), val);
        }

        public override void Emit(CodeGen cg) {
            cg.EmitGet(name, !defined);
        }

        public override void EmitSet(CodeGen cg) {
            cg.EmitSet(name);
        }

        public override void EmitDel(CodeGen cg) {
            cg.EmitDel(name, !defined);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                ;
            }
            w.PostWalk(this);
        }
    }


    public class AndExpr : Expr {
        public readonly Expr left, right;
        public AndExpr(Expr left, Expr right) {
            this.left = left;
            this.right = right;
            this.start = left.start;
            this.end = right.end;
        }

        public override object Evaluate(NameEnv env) {
            object ret = left.Evaluate(env);
            if (Ops.IsTrue(ret)) return right.Evaluate(env);
            else return ret;
        }

        public override void Emit(CodeGen cg) {
            left.Emit(cg);
            cg.Emit(OpCodes.Dup);
            cg.EmitCall(typeof(Ops), "IsTrue");
            //cg.emitNonzero(left);
            Label l = cg.DefineLabel();
            cg.Emit(OpCodes.Brfalse, l);
            cg.Emit(OpCodes.Pop);
            right.Emit(cg);
            cg.MarkLabel(l);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                left.Walk(w);
                right.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class OrExpr : Expr {
        public readonly Expr left, right;
        public OrExpr(Expr left, Expr right) {
            this.left = left; this.right = right;
            this.start = left.start; this.end = right.end;
        }

        public override object Evaluate(NameEnv env) {
            object ret = left.Evaluate(env);
            if (!Ops.IsTrue(ret)) return right.Evaluate(env);
            else return ret;
        }

        public override void Emit(CodeGen cg) {
            left.Emit(cg);
            cg.Emit(OpCodes.Dup);
            cg.EmitCall(typeof(Ops), "IsTrue");
            //cg.emitNonzero(left);
            Label l = cg.DefineLabel();
            cg.Emit(OpCodes.Brtrue, l);
            cg.Emit(OpCodes.Pop);
            right.Emit(cg);
            cg.MarkLabel(l);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                left.Walk(w);
                right.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class UnaryExpr : Expr {
        public readonly Expr expr;
        public readonly UnaryOperator op;
        public UnaryExpr(UnaryOperator op, Expr expr) {
            this.op = op; this.expr = expr;
            this.end = expr.end;
        }

        public override object Evaluate(NameEnv env) {
            return op.Evaluate(expr.Evaluate(env));
        }

        public override void Emit(CodeGen cg) {
            expr.Emit(cg);
            cg.EmitCall(op.target.Method);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                expr.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class BinaryExpr : Expr {
        public readonly Expr left, right;
        public readonly BinaryOperator op;
        public BinaryExpr(BinaryOperator op, Expr left, Expr right) {
            this.op = op; this.left = left; this.right = right;
            this.start = left.start; this.end = right.end;
        }

        public override object Evaluate(NameEnv env) {
            //!!! not right for compare
            object l = left.Evaluate(env);
            object r = right.Evaluate(env);

            return op.Evaluate(l, r);
        }

        public override void Emit(CodeGen cg) {
            left.Emit(cg);
            if (IsComparision() && IsComparision(right)) {
                FinishCompare(cg);
            } else {
                right.Emit(cg);
                op.Emit(cg);
            }
        }

        protected bool IsComparision() {
            return op.IsComparision();
        }

        public static bool IsComparision(Expr e) {
            BinaryExpr be = e as BinaryExpr;
            return be != null && be.IsComparision();
        }

        //!!! code review
        protected void FinishCompare(CodeGen cg) {
            BinaryExpr bright = (BinaryExpr)right;

            Slot valTmp = cg.GetLocalTmp(typeof(object));
            Slot retTmp = cg.GetLocalTmp(typeof(object));
            bright.left.Emit(cg);
            cg.Emit(OpCodes.Dup);
            valTmp.EmitSet(cg);

            cg.EmitCall(op.target.Method);
            cg.Emit(OpCodes.Dup);
            retTmp.EmitSet(cg);
            cg.EmitTestTrue();

            Label end = cg.DefineLabel();
            cg.Emit(OpCodes.Brfalse, end);

            valTmp.EmitGet(cg);

            if (IsComparision(bright.right)) {
                bright.FinishCompare(cg);
            } else {
                bright.right.Emit(cg);
                cg.EmitCall(bright.op.target.Method);
            }

            retTmp.EmitSet(cg);
            cg.MarkLabel(end);
            retTmp.EmitGet(cg);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                left.Walk(w);
                right.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class LambdaExpr : Expr {
        public readonly FuncDef func;
        public LambdaExpr(FuncDef func) {
            this.func = func;
        }

        public override object Evaluate(NameEnv env) {
            return func.MakeFunction(env);
        }

        public override void Emit(CodeGen cg) {
            func.Emit(cg);
            cg.EmitGet(func.name, false);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                func.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public abstract class ListCompIter : Node {
    }

    public class ListCompFor : ListCompIter {
        public readonly Expr lhs, list;

        public ListCompFor(Expr lhs, Expr list) {
            this.lhs = lhs; this.list = list;
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                lhs.Walk(w);
                list.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class ListCompIf : ListCompIter {
        public readonly Expr test;

        public ListCompIf(Expr test) {
            this.test = test;
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                test.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class ListComp : Expr {
        public readonly Expr item;
        public readonly ListCompIter[] iters;

        public ListComp(Expr item, ListCompIter[] citers) {
            this.item = item; this.iters = citers;
        }

        public override void Emit(CodeGen cg) {
            Slot list = cg.GetLocalTmp(typeof(List));
            cg.EmitCall(typeof(Ops), "MakeList", Type.EmptyTypes);
            list.EmitSet(cg);

            // first loop: how many For; initialize labels/slots
            int iFors = 0;
            foreach (ListCompIter iter in iters) {
                if (iter is ListCompFor) iFors++;
            }

            Label[] continueTargets = new Label[iFors];
            Slot[] enumerators = new Slot[iFors];
            int jIters = iters.Length;
            Label[] exitTargets = new Label[jIters];

            for (int i = 0; i < iFors; i++) {
                continueTargets[i] = cg.DefineLabel();
                enumerators[i] = cg.GetLocalTmp(typeof(IEnumerator));
            }
            for (int i = 0; i < jIters; i++) {
                exitTargets[i] = cg.DefineLabel();
            }

            // second loop: before emiting item
            iFors = jIters = 0;
            foreach (ListCompIter iter in iters) {
                if (iter is ListCompFor) {
                    ListCompFor cfor = iter as ListCompFor;
                    cfor.list.Emit(cg);
                    cg.EmitCall(typeof(Ops), "GetEnumerator");
                    enumerators[iFors].EmitSet(cg);

                    cg.MarkLabel(continueTargets[iFors]);

                    enumerators[iFors].EmitGet(cg);
                    cg.EmitCall(typeof(IEnumerator), "MoveNext", Type.EmptyTypes);
                    cg.Emit(OpCodes.Brfalse, exitTargets[jIters]);

                    enumerators[iFors].EmitGet(cg);
                    cg.EmitCall(typeof(IEnumerator).GetProperty("Current").GetGetMethod());

                    cfor.lhs.EmitSet(cg);
                    iFors++;
                } else if (iter is ListCompIf) {
                    ListCompIf cif = iter as ListCompIf;

                    cg.EmitTestTrue(cif.test);
                    cg.Emit(OpCodes.Brfalse, exitTargets[jIters]);
                }

                jIters++;
            }

            // append the item
            list.EmitGet(cg);
            this.item.Emit(cg);
            cg.EmitCall(typeof(List), "Append");

            // third loop: in reverse order
            iFors = continueTargets.Length - 1;
            jIters = iters.Length - 1;
            while (jIters >= 0) {
                ListCompIter iter = iters[jIters];
                if (iter is ListCompFor) {
                    cg.Emit(OpCodes.Br, continueTargets[iFors]);
                    cg.FreeLocalTmp(enumerators[iFors]);
                    iFors--;
                }

                cg.MarkLabel(exitTargets[jIters]);
                jIters--;
            }

            list.EmitGet(cg);
            cg.FreeLocalTmp(list);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                foreach (ListCompIter iter in iters) iter.Walk(w);

                item.Walk(w);
            }
            w.PostWalk(this);
        }
    }

    public class GenExpr : Expr {
        public readonly FuncDef func;
        public readonly CallExpr call;

        public GenExpr(FuncDef func, CallExpr call) {
            this.func = func;
            this.call = call;
        }

        public override void Emit(CodeGen cg) {
            func.Emit(cg);
            call.Emit(cg);
        }

        public override void Walk(IAstWalker w) {
            if (w.Walk(this)) {
                func.Walk(w);
                call.Walk(w);
            }
            w.PostWalk(this);
        }
    }
}
