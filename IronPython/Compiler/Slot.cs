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
using System.Collections.Generic;

using System.Diagnostics;

using IronPython.Runtime;

namespace IronPython.Compiler {

    // Slot refers to a reference to an object. For eg, a global variable, a local variable, etc.
    // A Slot is referred to using a Name. The Namespace is used to map a Name to a Slot.
    // Multiple Names can refer to the same Slot.
    // For eg. multiple closures can refer to the same Slot of a local variable in the enclosing
    // function. Though each closure will use the same string (the name of the variable), each
    // string is considered a unique Name or symbol.

    public abstract class Slot {
        private bool local;

        internal bool Local {
            get { return local; }
            set { local = value; }
        }

        public abstract void EmitGet(CodeGen cg);
        public abstract void EmitGetAddr(CodeGen cg);

        //!!! must override at least one of these two methods or get infinite loop
        public virtual void EmitSet(CodeGen cg, Slot val) {
            val.EmitGet(cg);
            EmitSet(cg);
        }

        // This override assumes that the IL stack already holds the value to be assigned from.
        public virtual void EmitSet(CodeGen cg) {
            // localTmpVal = <top of IL stack>
            Slot localTmpVal = cg.GetLocalTmp(typeof(object));
            localTmpVal.EmitSet(cg);

            // <slot> = localTmpVal
            EmitSet(cg, localTmpVal);

            cg.FreeLocalTmp(localTmpVal);
        }

        // Slots are to be implemented as dictionaries for Python. However,
        // for performance and better integration with the CLI, the engine
        // implements many Slot types as CLI entities, which cannot be
        // deleted once they are created. Hence, to implement "del",
        // an alternate scheme is used. We just assign an instance of the
        // type "Uninitialized" to represent that the Slot has been deleted.
        // Any access to the Slot first checks if it is holding an "Uninitialized",
        // which means that it should virtually not exist

        public virtual void EmitSetUninitialized(CodeGen cg, Name name) {
            // Emit the following:
            //     <name> = new Uninitialized("<name>");
            // Including "name" helps with debugging
            cg.EmitName(name);
            cg.EmitNew(typeof(Uninitialized), new Type[] { typeof(string) });
            EmitSet(cg);
        }

        public virtual void EmitDelete(CodeGen cg, Name name, bool check) {
            // First check that the Name exists. Otherwise, deleting it
            // should cause a NameError
            if (check && Options.CheckInitialized) {
                EmitGet(cg);
                EmitCheck(cg);
                cg.Emit(OpCodes.Pop);
            }

            EmitSetUninitialized(cg, name);
        }

        public virtual void EmitCheck(CodeGen cg) {
            //!!! the two lines below add a 8% overhead to pystone
            //!!! omit them when we can prove they are not needed
            if (Options.CheckInitialized) {
                cg.Emit(OpCodes.Dup);
                if (local) {
                    cg.EmitCall(typeof(Ops), "CheckInitializedLocal");
                } else {
                    cg.EmitCall(typeof(Ops), "CheckInitialized");
                }
            }
        }

        public abstract Type Type { get; }
    }

    // NamedFrameSlot represens a global variables (or builtin) of FrameCode code executing 
    // in the context of a Frame. They have to be looked up by name at runtime.

    public sealed class NamedFrameSlot : Slot {
        // The Frame whose Namespace will be used to resolve the Name
        private readonly Slot frame;
        private readonly Name name;

        public NamedFrameSlot(Slot frame, Name name) {
            Debug.Assert(typeof(IFrameEnvironment).IsAssignableFrom(frame.Type), "invalid frame type");
            this.frame = frame;
            this.name = name;
        }

        public override void EmitGet(CodeGen cg) {
            frame.EmitGet(cg);
            cg.EmitString(name.GetString());
            cg.EmitCall(typeof(IFrameEnvironment), "GetGlobal");
        }

        public override void EmitGetAddr(CodeGen cg) {
            //???how bad is it that we can't do this???
            throw new NotImplementedException("address of frame slot");
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            // Emit the following:
            //    frame.SetGlobal("name", val)
            frame.EmitGet(cg);
            cg.EmitString(name.GetString());
            val.EmitGet(cg);
            cg.EmitCall(typeof(IFrameEnvironment), "SetGlobal");
        }

        public override void EmitSetUninitialized(CodeGen cg, Name name) {
        }

        public override void EmitDelete(CodeGen cg, Name name, bool check) {
            // Emit the following:
            //    frame.DelGlobal("name")
            frame.EmitGet(cg);
            cg.EmitString(name.GetString());
            cg.EmitCall(typeof(IFrameEnvironment), "DelGlobal");
        }

        public override Type Type {
            get {
                return typeof(object);
            }
        }
    }

    public class LocalNamedFrameSlot : Slot {
        public readonly Slot frame;
        public readonly Name name;

        public LocalNamedFrameSlot(Slot frame, Name name) {
            this.frame = frame;
            this.name = name;
        }

        public override void EmitGet(CodeGen cg) {
            frame.EmitGet(cg);
            cg.EmitString(name.GetString());
            cg.EmitCall(typeof(Frame), "GetLocal");
        }

        public override void EmitGetAddr(CodeGen cg) {
            //???how bad is it that we can't do this???
            throw new NotImplementedException("address of local frame slot");
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            // Emit the following:
            //    frame.SetName("name", val)
            frame.EmitGet(cg);
            cg.EmitString(name.GetString());
            val.EmitGet(cg);
            cg.EmitCall(typeof(Frame), "SetLocal");
        }

        public override void EmitSetUninitialized(CodeGen cg, Name name) {
        }

        public override void EmitDelete(CodeGen cg, Name name, bool check) {
            // Emit the following:
            //    frame.DelName("name")
            frame.EmitGet(cg);
            cg.EmitString(name.GetString());
            cg.EmitCall(typeof(Frame), "DelLocal");
        }

        public override Type Type {
            get {
                return typeof(object);
            }
        }

    }

    // Most imported Python modules uses static fields in an assembly for the module globals.

    public class StaticFieldSlot : Slot {
        public readonly FieldInfo field;

        public StaticFieldSlot(FieldInfo field) {
            this.field = field;
        }
        public override void EmitGet(CodeGen cg) {
            cg.EmitFieldGet(field);
        }
        public override void EmitGetAddr(CodeGen cg) {
            cg.Emit(OpCodes.Ldsflda, field);
        }

        public override void EmitSet(CodeGen cg) {
            cg.EmitFieldSet(field);
        }

        public override Type Type {
            get {
                return field.FieldType;
            }
        }
    }

    /// <summary>
    /// A StaticField Slot who's name collides w/ a built-in name
    /// </summary>
    public class StaticFieldBuiltinSlot : StaticFieldSlot {
        public StaticFieldBuiltinSlot(FieldInfo field)
            : base(field) {
        }

        public override void EmitGet(CodeGen cg) {
            base.EmitGet(cg);
            cg.EmitCall(typeof(BuiltinWrapper), "get_CurrentValue");
        }

        public void EmitGetRaw(CodeGen cg) {
            base.EmitGet(cg);
            cg.Emit(OpCodes.Ldfld, typeof(BuiltinWrapper).GetField("currentValue"));
        }

        public override void EmitGetAddr(CodeGen cg) {
            base.EmitGet(cg);
            cg.Emit(OpCodes.Ldflda, typeof(BuiltinWrapper).GetField("currentValue"));
        }

        public override void EmitSet(CodeGen cg) {
            Slot val = cg.GetLocalTmp(Type);
            val.EmitSet(cg);

            base.EmitGet(cg);
            val.EmitGet(cg);
            cg.EmitCall(typeof(BuiltinWrapper), "set_CurrentValue");

            cg.FreeLocalTmp(val);
        }

        public override Type Type {
            get {
                return typeof(object);
            }
        }
    }

    // FieldSlot is an access of an attribute of an object
    public class FieldSlot : Slot {
        public readonly Slot instance;
        public readonly FieldInfo field;

        public FieldSlot(Slot instance, FieldInfo field) {
            this.instance = instance;
            this.field = field;
        }
        public override void EmitGet(CodeGen cg) {
            instance.EmitGet(cg);
            cg.Emit(OpCodes.Ldfld, field);
        }
        public override void EmitGetAddr(CodeGen cg) {
            instance.EmitGet(cg);
            cg.Emit(OpCodes.Ldflda, field);
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            instance.EmitGet(cg);
            val.EmitGet(cg);
            cg.Emit(OpCodes.Stfld, field);
        }

        public override void EmitSet(CodeGen cg) {
            Slot val = cg.GetLocalTmp(field.FieldType);
            val.EmitSet(cg);
            EmitSet(cg, val);
            cg.FreeLocalTmp(val);
        }

        public override Type Type {
            get {
                return field.FieldType;
            }
        }
    }

    // Local variable access
    // Note that access of local variables of an enclosing function is done using a FieldSlot

    public class LocalSlot : Slot {
        public readonly LocalBuilder localBuilder;
        private readonly CodeGen codeGen;           // LocalSlot's can only be used w/ codegen that created them

        public LocalSlot(LocalBuilder localBuilder, CodeGen cg) {
            this.localBuilder = localBuilder;
            codeGen = cg;
        }
        public override void EmitGet(CodeGen cg) {
            Debug.Assert(cg == codeGen);

            cg.Emit(OpCodes.Ldloc, localBuilder);
        }
        public override void EmitGetAddr(CodeGen cg) {
            Debug.Assert(cg == codeGen);

            cg.Emit(OpCodes.Ldloca, localBuilder);
        }

        public override void EmitSet(CodeGen cg) {
            Debug.Assert(cg == codeGen);
            cg.Emit(OpCodes.Stloc, localBuilder);
        }

        public override Type Type {
            get { return localBuilder.LocalType; }
        }
    }

    // Argument access
    public class ArgSlot : Slot {
        private Type argType;
        private int index;
        private CodeGen codeGen;

        public ArgSlot(int index, Type type, CodeGen codeGen) {
            this.index = index;
            this.argType = type;
            this.codeGen = codeGen;
        }

        public void SetName(string name) {
            codeGen.DefineParameter(index, ParameterAttributes.None, name);
        }


        public override void EmitGet(CodeGen cg) {
            //Debug.Assert(cg == this.codeGen);
            cg.EmitTrueArgGet(index);
        }

        public override void EmitGetAddr(CodeGen cg) {
            Debug.Assert(cg == this.codeGen);
            cg.Emit(OpCodes.Ldarga, index);
        }

        public override void EmitSet(CodeGen cg) {
            Debug.Assert(cg == this.codeGen);
            cg.Emit(OpCodes.Starg, index);
        }

        public override Type Type {
            get {
                return argType;
            }
        }
    }

    class ParamArraySlot : Slot{
        Slot param;
        int index;

        public ParamArraySlot(Slot paramArray, int paramIndex){
            param = paramArray;
            index = paramIndex;
        }

        public override void EmitGet(CodeGen cg) {
            param.EmitGet(cg);
            cg.EmitInt(index);
            cg.Emit(OpCodes.Ldelem_Ref);
        }

        public override void EmitGetAddr(CodeGen cg) {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override Type Type {
            get { return typeof(object); }
        }
    }

    // Accessing self
    public class ThisSlot : Slot {
        public readonly Type type;

        public ThisSlot(Type type) {
            this.type = type;
        }
        public override void EmitGet(CodeGen cg) {
            cg.EmitThis();
        }

        public override void EmitSet(CodeGen cg) {
            throw new InvalidOperationException("setting this");
        }

        public override void EmitGetAddr(CodeGen cg) {
            cg.Emit(OpCodes.Ldarga, 0);
        }

        public override Type Type {
            get {
                return type;
            }
        }
    }

    public class VTableSlot : Slot {
        public readonly Slot typeSlot;
        public readonly string name;
        public readonly int index;

        public VTableSlot(Slot typeSlot, string name, int index) {
            this.typeSlot = typeSlot;
            this.name = name;
            this.index = index;
        }

        public override void EmitGet(CodeGen cg) {
            throw new NotImplementedException("not much of a slot");
        }

        public void EmitTryGetNonInheritedValue(CodeGen cg, Slot valueOut) {
            typeSlot.EmitGet(cg);
            cg.EmitCall(typeof(UserType), "GetNamespaceDictionary");
            cg.EmitInt(index);
            valueOut.EmitGetAddr(cg);
            cg.EmitCall(typeof(NamespaceDictionary), "TryGetNonInheritedValue");
        }

        public override void EmitGetAddr(CodeGen cg) {
            //???we can do this
            throw new NotImplementedException("address of vtable slot");
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            throw new NotImplementedException("shouldn't need this");
        }

        public override Type Type {
            get {
                return typeof(object);
            }
        }
    }

    public class IndexSlot : Slot {
        private Slot instance;
        private int index;
        private Type type;

        public IndexSlot(Slot instance, int index)
            : this(instance, index, typeof(object)) {
        }

        public IndexSlot(Slot instance, int index, Type type) {
            this.instance = instance;
            this.index = index;
            this.type = type;
        }

        public int Index {
            get {
                return index;
            }
        }

        public override void EmitGet(CodeGen cg) {
            instance.EmitGet(cg);
            cg.EmitInt(index);
            cg.Emit(OpCodes.Ldelem_Ref);
            cg.EmitCastFromObject(Type);
        }

        public override void EmitSet(CodeGen cg) {
            Slot val = cg.GetLocalTmp(Type);
            val.EmitSet(cg);
            EmitSet(cg, val);
            cg.FreeLocalTmp(val);
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            val.EmitGet(cg);
            cg.EmitCastToObject(Type);
            val = cg.GetLocalTmp(typeof(object));
            val.EmitSet(cg);
            instance.EmitGet(cg);
            cg.EmitInt(index);
            val.EmitGet(cg);
            cg.Emit(OpCodes.Stelem_Ref);
        }

        public override void EmitGetAddr(CodeGen cg) {
            instance.EmitGet(cg);
            cg.EmitInt(index);
            cg.Emit(OpCodes.Ldelema, Type);
        }

        public override Type Type {
            get {
                return type;
            }
        }
    }

    public class PropertySlot : Slot {
        Slot instance;
        PropertyInfo property;

        public PropertySlot(Slot instance, PropertyInfo property) {
            this.instance = instance;
            this.property = property;
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            MethodInfo method = property.GetSetMethod();
            Debug.Assert(method != null, "Cannot set property");
            Debug.Assert(method.GetParameters().Length == 1, "Wrong number of parameters on the property setter");

            //  Emit instance
            if (!method.IsStatic) {
                Debug.Assert(instance != null, "need instance slot for instance property");
                instance.EmitGet(cg);
            }

            //  Emit value
            val.EmitGet(cg);

            //  Emit call
            cg.EmitCall(method);
        }

        public override void EmitGet(CodeGen cg) {
            MethodInfo method = property.GetGetMethod();
            Debug.Assert(method != null, "Cannot set property");
            Debug.Assert(method.GetParameters().Length == 0, "Wrong number of parameters on the property getter");

            // Emit instance
            if (!method.IsStatic) {
                Debug.Assert(instance != null, "need instance slot for instance property");
                instance.EmitGet(cg);
            }

            // Emit call
            cg.EmitCall(method);
        }

        public override void EmitGetAddr(CodeGen cg) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override Type Type {
            get {
                return property.PropertyType;
            }
        }
    }

    internal class CastSlot : Slot {
        private Slot instance;
        private Type type;

        public CastSlot(Slot instance, Type type) {
            this.instance = instance;
            this.type = type;
        }

        public override void EmitGet(CodeGen cg) {
            instance.EmitGet(cg);
            if (!type.IsAssignableFrom(instance.Type)) {
                cg.Emit(OpCodes.Castclass, type);
            }
        }

        public override void EmitGetAddr(CodeGen cg) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void EmitSet(CodeGen cg) {
            if (instance.Type.IsAssignableFrom(type)) {
                cg.Emit(OpCodes.Castclass, instance.Type);
            }
            instance.EmitSet(cg);
        }

        public override Type Type {
            get {
                return type;
            }
        }
    }

    internal class GlobalBackedSlot : Slot {
        Slot attribute;
        Slot global;

        public GlobalBackedSlot(Slot attribute, Slot global) {
            this.attribute = attribute;
            this.global = global;
        }

        public override void EmitGet(CodeGen cg) {
            attribute.EmitGet(cg);
        }

        public override void EmitCheck(CodeGen cg) {
            Label initialized = cg.DefineLabel();
            cg.Emit(OpCodes.Dup);
            cg.Emit(OpCodes.Isinst, typeof(Uninitialized));
            cg.Emit(OpCodes.Brfalse, initialized);
            cg.Emit(OpCodes.Pop);
            global.EmitGet(cg);
            global.EmitCheck(cg);
            cg.MarkLabel(initialized);
        }

        public override void EmitDelete(CodeGen cg, Name name, bool check) {
            attribute.EmitDelete(cg, name, check);
        }

        public override void EmitSet(CodeGen cg) {
            attribute.EmitSet(cg);
        }

        public override void EmitSet(CodeGen cg, Slot val) {
            attribute.EmitSet(cg, val);
        }

        public override void EmitSetUninitialized(CodeGen cg, Name name) {
            attribute.EmitSetUninitialized(cg, name);
        }

        public override void EmitGetAddr(CodeGen cg) {
            attribute.EmitGetAddr(cg);
        }

        public override Type Type {
            get { return typeof(object); }
        }
    }
}
