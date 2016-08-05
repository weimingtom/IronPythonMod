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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Modules;
using IronMath;

namespace IronPython.Runtime {
    //public enum VTableSource {
    //    Declared, Inherited, InheritedFromObject
    //}

    /// <summary>
    /// UserType represents the type of new-style Python classes (which can inherit from built-in types). 
    /// 
    /// Object instances of new-style Python classes are represented by classes generated by NewTypeMaker,
    /// and are named IronPython.NewTypes.someName.
    /// 
    /// OldClass is the equivalent of UserType for old-style Python classes (which cannot inherit from 
    /// built-in types).
    /// </summary>

    [DebuggerDisplay("UserType: {ToString()}")]
    [PythonType(typeof(PythonType))]
    public partial class UserType : PythonType, IFancyCallable, IWeakReferenceable, ICallableWithCallerContext {
        // This is typed as "object" instead of "string" as the user is allowed to set it to an arbitrary object
        public object __module__;
        private Tuple bases;

        #region Public API Surface

        public static UserType MakeClass(string name, Tuple bases, IDictionary<object, object> dict) {
            return new UserType(name, bases, dict);
        }

        /// <summary>
        /// called from generated code (VTableSlot)
        /// </summary>
        public NamespaceDictionary GetNamespaceDictionary() {
            //Console.WriteLine("dict = " + dict);
            return (NamespaceDictionary)dict;
        }

        #endregion

        #region Constructors

        protected UserType(string name, Tuple bases, IDictionary<object, object> dict)
            : base(NewTypeMaker.GetNewType(name, bases, dict)) {
            List<MethodInfo> ctors = new List<MethodInfo>();
            foreach (MethodInfo mi in type.GetMethods()) {
                if (mi.Name == ReflectedType.MakeNewName) ctors.Add(mi);
            }

            if (ctors.Count == 0) throw new NotImplementedException("no MakeNew found");

            ctor = BuiltinFunction.Make(name, ctors.ToArray(), ctors.ToArray(), FunctionType.Function);

            IAttributesDictionary fastDict = (IAttributesDictionary)dict;

            this.__name__ = name;
            this.__module__ = fastDict[SymbolTable.Module];   // should always be present...

            if (!fastDict.ContainsKey(SymbolTable.Doc)) {
                fastDict[SymbolTable.Doc] = null;
            }

            InitializeUserType(bases, false);

            this.dict = CreateNamespaceDictionary(dict);

            AddProtocolWrappers();
        }

        /// <summary>
        /// Set up the type
        /// </summary>
        /// <param name="resetType">Is an existing type being reset?</param>
        void InitializeUserType(Tuple bases, bool resetType) {
            if (resetType) {
                foreach (object baseTypeObj in BaseClasses) {
                    if (baseTypeObj is OldClass) continue;
                    PythonType baseType = baseTypeObj as PythonType;
                    baseType.RemoveSubclass(this);
                }
            }

            this.bases = EnsureBaseType(bases);

            for (int i = 0; i < this.bases.Count; i++) {
                for (int j = 0; j < this.bases.Count; j++) {
                    if (i != j && this.bases[i] == this.bases[j]) {
                        throw Ops.TypeError("duplicate base class {0}", ((DynamicType)this.bases[i]).__name__);
                    }
                }
            }

            foreach (object baseTypeObj in BaseClasses) {
                if (baseTypeObj is OldClass) continue;
                PythonType baseType = baseTypeObj as PythonType;
                baseType.AddSubclass(this);
            }

            if (!resetType)
                Initialize();
        }

        #endregion

        #region PythonType overrides

        public override Type GetTypesToExtend(out IList<Type> interfacesToExtend) {
            interfacesToExtend = new List<Type>();
            foreach (object b in bases) {
                if (b is OldClass) continue;

                PythonType baseType = b as PythonType;
                IList<Type> baseTypeInterfaces;
                baseType.GetTypesToExtend(out baseTypeInterfaces);
                foreach (Type baseTypeInterface in baseTypeInterfaces)
                    interfacesToExtend.Add(baseTypeInterface);
            }
            // We dont use type.GetInterfaces() as it contains all the interfaces that are added by NewTypeMaker,
            // as well as all the interfaces implemented by type.BaseType. Instead, we only want the new set of
            // interfaces that need to be implemented by the new instance type.
            Debug.Assert(interfacesToExtend.Count < type.GetInterfaces().Length);

            // "type" is the instance type used for instances of this type. This will be a type created by NewTypeMaker. 
            Debug.Assert(NewTypeMaker.IsInstanceType(type));
            // Its BaseType will the CLI type that needs to be inherited by instance types used by other UserTypes inheriting 
            // from the current UserType. For pure Python types, this will be System.Object.
            Debug.Assert(!NewTypeMaker.IsInstanceType(type.BaseType));

            return type.BaseType;
        }

        public override Tuple BaseClasses {
            [PythonName("__bases__")]
            get { return bases; }

            [PythonName("__bases__")]
            set {
                foreach (DynamicType baseType in bases) {
                    if (baseType is OldClass) continue;
                    if (baseType is UserType) continue;
                    if (!value.ContainsValue(baseType)) throw Ops.TypeError("cannot remove CLI type {0} from {1}.__bases__", baseType, this);
                }

                foreach (DynamicType baseType in value) {
                    if (baseType is OldClass) continue;
                    if (baseType is UserType) continue;
                    if (!bases.ContainsValue(baseType)) throw Ops.TypeError("cannot add CLI type {0} to {1}.__bases__", baseType, this);
                }

                Tuple newMRO = CalculateMro(value);

                lock (this) {
                    InitializeUserType(value, true);

                    // note: bases & MethodResolutionOrder are out of sync for a short period of time.  But because 
                    // the user cannot atomically read both  values at the same time, this is logically the same as the race 
                    // happening in between those reads.  The important thing is that bases.
                    // Same thing for all of our __* methods that are cached w/ MethodWrappers.

                    MethodResolutionOrder = newMRO;

                    UpdateFromBases();
                }
            }
        }

        protected override string TypeCategoryDescription {
            get {
                return "user-defined class";
            }
        }

        #endregion

        #region DynamicType overrides

        public override string Repr(object self) {
            if (__repr__F.IsObjectMethod()) {
                return self.ToString();
            } else {
                return (string)__repr__F.Invoke(self);
            }
        }

        public override bool IsSubclassOf(object other) {
            ReflectedType rt = other as ReflectedType;
            if (rt != null) {
                if (type == rt.type || type.IsSubclassOf(rt.type))
                    return true;

                Type otherTypeToExtend = rt.GetTypeToExtend();
                if (otherTypeToExtend != null) {
                    if (type == otherTypeToExtend || type.IsSubclassOf(otherTypeToExtend))  //!!! inefficient in most cases
                        return true;

                    foreach (Type interfaceType in type.GetInterfaces()) {
                        if (interfaceType == otherTypeToExtend)
                            return true;
                    }
                }

                return false;
            }

            if (this.Equals(other)) return true;

            foreach (DynamicType baseType in BaseClasses) {
                if (baseType.IsSubclassOf(other)) return true;
            }
            return false;
        }

        public override object GetAttr(ICallerContext context, object self, SymbolId name) {
            if (__getattribute__F.IsObjectMethod()) {
                object ret;
                if (TryBaseGetAttr(context, (ISuperDynamicObject)self, name, out ret)) {
                    return ret;
                } else {
                    throw Ops.AttributeError((string)SymbolTable.IdToString(name));
                }
            } else {
                return __getattribute__F.Invoke(self, SymbolTable.IdToString(name));
            }
        }

        public override bool TryGetAttr(ICallerContext context, object self, SymbolId name, out object ret) {
            if (__getattribute__F.IsObjectMethod()) {
                ISuperDynamicObject sdo = self as ISuperDynamicObject;
                if (sdo != null) {
                    return TryBaseGetAttr(context, sdo, name, out ret);
                } else {
                    ret = null;
                    return false;
                }
            } else {
                try {
                    ret = __getattribute__F.Invoke(self, SymbolTable.IdToString(name));
                    return true;
                } catch (MissingMemberException) {
                    ret = null;
                    return false;
                }
            }
        }

        public override void SetAttr(ICallerContext context, object self, SymbolId name, object value) {
            if (name == SymbolTable.Class) {
                // check that this is a legal new class
                UserType newType = value as UserType;
                if (newType == null) {
                    throw Ops.TypeError("__class__ must be set to new-style class, not '{0}' object", Ops.GetDynamicType(value).__name__);
                }
                if (newType.type != this.type) {
                    throw Ops.TypeError("__class__ assignment: '{0}' object layout differs from '{1}'", __name__, newType.__name__);
                }
                ((ISuperDynamicObject)self).SetDynamicType(newType);
                return;
            }

            if (__setattr__F.IsObjectMethod()) {
                BaseSetAttr(context, (ISuperDynamicObject)self, name, value);
            } else {
                __setattr__F.Invoke(self, SymbolTable.IdToString(name), value);
            }
        }

        public override void DelAttr(ICallerContext context, object self, SymbolId name) {
            if (__delattr__F.IsObjectMethod()) {
                BaseDelAttr(context, (ISuperDynamicObject)self, name);
            } else {
                __delattr__F.Invoke(self, SymbolTable.IdToString(name));
            }
        }

        public void BaseDelAttr(ICallerContext context, ISuperDynamicObject self, SymbolId name) {
            object slot;
            if (!TryLookupSlot(context, name, out slot)) {
                IAttributesDictionary d = GetInitializedDict((ISuperDynamicObject)self); //!!! forces dict init
                if (d.ContainsKey(name)) {
                    d.Remove(name);
                    return;
                } else {
                    throw Ops.AttributeError("no slot named {0} on {1}", SymbolTable.IdToString(name), this.__name__);
                }
            }
            Ops.DelDescriptor(slot, self);
        }

        public override List GetAttrNames(ICallerContext context, object self) {
            List baseNames = base.GetAttrNames(context, self);

            ISuperDynamicObject sdo = self as ISuperDynamicObject;
            if (sdo != null) {
                IAttributesDictionary dict = sdo.GetDict();
                if (dict != null) {
                    foreach (object o in dict.Keys) {
                        if (!baseNames.Contains(o)) baseNames.Add(o);
                    }
                }
            }

            return baseNames;
        }

        #endregion

        #region Object overrides

        [PythonName("__str__")]
        public override string ToString() {
            return string.Format("<class '{0}.{1}'>", __module__, __name__);
        }

        #endregion

        #region Internal implementation

        //!!! private would be better, only called from PythonType
        internal bool TryBaseGetAttr(ICallerContext context, ISuperDynamicObject self, SymbolId name, out object ret) {
            if (name == SymbolTable.Dict) {
                ret = GetInitializedDict(self);
                return true;
            }

            IAttributesDictionary d = self.GetDict();
            if (d != null) {
                if (d.TryGetValue(name, out ret)) {
                    return true;
                }
            }

            if (TryLookupSlot(context, name, out ret)) {
                ret = Ops.GetDescriptor(ret, self, this);
                return true;
            }

            if (name == SymbolTable.Class) { ret = this; return true; }
            if (name == SymbolTable.WeakRef) { ret = null; return true; }

            if (!__getattr__F.IsObjectMethod()) {
                ret = __getattr__F.Invoke(self, SymbolTable.IdToString(name));
                return true;
            }

            ret = null;
            return false;
        }

        //!!! private would be better, only called from PythonType
        internal void BaseSetAttr(ICallerContext context, ISuperDynamicObject self, SymbolId name, object value) {
            object slot;
            if (TryLookupSlot(context, name, out slot)) {
                if (Ops.SetDescriptor(slot, self, value)) return;
            }

            if (name == SymbolTable.WeakRef) throw Ops.TypeError("attribute '__weakref__' of '{0}' objects is not writable", __name__);

            IAttributesDictionary d = GetInitializedDict(self);
            d[name] = value;
        }

        #endregion

        #region Private implementation details
        private NamespaceDictionary CreateNamespaceDictionary(IDictionary<object, object> dict) {
            string[] names = (string[])this.type.GetField(NewTypeMaker.VtableNamesField).GetValue(null);
            SymbolId[] symNames = new SymbolId[names.Length];
            for (int i = 0; i < symNames.Length; i++) {
                symNames[i] = SymbolTable.StringToId(names[i]);
            }
            NamespaceDictionary ret = NamespaceDictionary.Make(symNames, bases);

            foreach (KeyValuePair<object, object> kv in dict) {
                PythonFunction func = kv.Value as PythonFunction;
                if (func != null && func.Name != "__new__") {
                    ret.AsObjectKeyedDictionary()[kv.Key] = new Method(func, null, this);
                } else {
                    ret.AsObjectKeyedDictionary()[kv.Key] = kv.Value;
                }
            }
            //!!! need invalidation, inheritance, all that good stuff
            return ret;
        }

        /// <summary>
        /// If we have only interfaces, we'll need to insert object's base
        /// </summary>
        private static Tuple EnsureBaseType(Tuple bases) {
            foreach (object baseClass in bases) {
                if (baseClass is OldClass) continue;

                ReflectedType reflectedBaseType = baseClass as ReflectedType;
                if (reflectedBaseType == null || !reflectedBaseType.GetTypeToExtend().IsInterface) {
                    // Found a concrete (non-interface) type. We are done.
                    return bases;
                }

            }

            // We found only interfaces. We need do add System.Object to the bases
            return new Tuple(bases, TypeCache.Object);
        }

        private static IAttributesDictionary GetInitializedDict(ISuperDynamicObject self) {
            IAttributesDictionary d = self.GetDict();
            if (d == null) {
                d = new FieldIdDict();
                self.SetDict(d);
            }
            return d;
        }

        #endregion

        #region IDynamicObject Members

        public override DynamicType GetDynamicType() {
            return Ops.GetDynamicTypeFromType(typeof(UserType));  //!!! should be stuck in a static somewhere for faster lookup
        }

        #endregion

        #region ICallableWithCallerContext Members

        object ICallableWithCallerContext.Call(ICallerContext context, object[] args) {
            object newMethod, newObject;


            newMethod = Ops.GetAttr(context, this, SymbolTable.NewInst);
            newObject = Ops.CallWithContext(context, newMethod, PrependThis(args));

            if (newObject == null) return null;

            if (Ops.GetDynamicType(newObject).IsSubclassOf(this)) {
                object init;
                if (PythonType.TryLookupSpecialMethod(DefaultContext.Default, newObject, SymbolTable.Init, out init)) {
                    switch (args.Length) {
                        case 0: Ops.CallWithContext(context, init); break;
                        case 1: Ops.CallWithContext(context, init, args[0]); break;
                        case 2: Ops.CallWithContext(context, init, args[0], args[1]); break;
                        default: Ops.CallWithContext(context, init, args); break;
                    }

                }
            }

            return newObject;
        }

        #endregion

        #region ICallable Members

        public override object Call(params object[] args) {
            return ((ICallableWithCallerContext)this).Call(DefaultContext.Default, args);
        }

        #region IFancyCallable Members

        public object Call(object[] args, string[] names) {
            object newMethod, newObject = null;
            if (TryGetAttr(DefaultContext.Default, SymbolTable.NewInst, out newMethod)) {
                IFancyCallable ifc = newMethod as IFancyCallable;
                if (ifc != null) {
                    newObject = ifc.Call(PrependThis(args), names);
                } else {
                    throw Ops.TypeError("{0} object is not callable", Ops.GetDynamicType(newMethod).__name__);
                }
            } else {
                Debug.Assert(names.Length != 0);
                throw Ops.TypeError("default __new__ takes no parameters");
            }

            if (newObject == null) return null;

            InvokeInit(newObject, args, names);

            return newObject;
        }

        #endregion

        #endregion

        #region IRichEquality helpers

        public static object RichGetHashCodeHelper(object self) {
            // new-style classes only lookup in slots, not in instance
            // members
            object func;
            if (PythonType.TryLookupSpecialMethod(self, SymbolTable.Hash, out func)) {
                return Converter.ConvertToInt32(Ops.Call(func));
            }
            return Ops.NotImplemented;
        }

        public static object RichEqualsHelper(object self, object other) {
            object res = InternalCompare(SymbolTable.OpEqual, self, other);
            if (res != Ops.NotImplemented) return res;

            return Ops.NotImplemented;
        }

        public static object RichNotEqualsHelper(object self, object other) {
            object res = InternalCompare(SymbolTable.OpNotEqual, self, other);
            if (res != Ops.NotImplemented) return res;

            return Ops.NotImplemented;
        }
        #endregion

        #region IRichComparable Helpers
        public static object CompareToHelper(object self, object other) {

            DynamicType selfType = Ops.GetDynamicType(self);
            DynamicType otherType = Ops.GetDynamicType(other);

            object res;
            if (selfType == otherType) {
                // try __cmp__ first if it's defined.
                res = InternalCompare(SymbolTable.Cmp, self, other);
                if (res != Ops.NotImplemented) return res;

                // no need to check the other side - the types are identical,
                // and we don't look on instances for new-style classes.
            }

            // next try equals, return 0 if we match.
            res = RichEqualsHelper(self, other);
            if (res != Ops.NotImplemented) {
                if (Ops.IsTrue(res)) return 0;
            } else if (other != null) {
                // try the reverse
                res = RichEqualsHelper(other, self);
                if (res != Ops.NotImplemented && Ops.IsTrue(res)) return 0;
            }

            // next try less than
            res = LessThanHelper(self, other);
            if (res != Ops.NotImplemented) {
                if (Ops.IsTrue(res)) return -1;
            } else if (other != null) {
                // try the reverse
                res = GreaterThanHelper(other, self);
                if (res != Ops.NotImplemented && Ops.IsTrue(res)) return -1;
            }

            // finally try greater than
            res = GreaterThanHelper(self, other);
            if (res != Ops.NotImplemented) {
                if (Ops.IsTrue(res)) return 1;
            } else if (other != null) {
                //and the reverse
                res = LessThanHelper(other, self);
                if (res != Ops.NotImplemented && Ops.IsTrue(res)) return 1;
            }

            if (selfType != otherType) {
                // finally try __cmp__ if our types are different
                res = InternalCompare(SymbolTable.Cmp, self, other);
                if (res != Ops.NotImplemented) return res;
            }

            return Ops.NotImplemented;
        }

        public static object GreaterThanHelper(object self, object other) {
            return InternalCompare(SymbolTable.OpGreaterThan, self, other);
        }

        public static object LessThanHelper(object self, object other) {
            return InternalCompare(SymbolTable.OpLessThan, self, other);
        }

        public static object GreaterThanOrEqualHelper(object self, object other) {
            return InternalCompare(SymbolTable.OpGreaterThanOrEqual, self, other);
        }

        public static object LessThanOrEqualHelper(object self, object other) {
            return InternalCompare(SymbolTable.OpLessThanOrEqual, self, other);
        }

        private static object InternalCompare(SymbolId cmp, object self, object other) {
            object meth;
            if (PythonType.TryLookupSpecialMethod(self, cmp, out meth)) {
                object ret;
                if (Ops.TryCall(meth, other, out ret)) {
                    return ret;
                }
            }
            return Ops.NotImplemented;
        }

        #endregion

        #region Object Override helpers

        /// <summary>
        /// Object.ToString() displays the CLI type name.  But we want to display the class name (e.g.
        /// '<foo object at 0x000000000000002C>' unless we've overridden __repr__ but not __str__ in 
        /// which case we'll display the result of __repr__.
        /// </summary>
        public static string ToStringHelper(ISuperDynamicObject o) {

            object ret;
            UserType ut = o.GetDynamicType() as UserType;
            if (ut.TryLookupSlot(DefaultContext.Default, SymbolTable.Repr, out ret)) {
                return Converter.ConvertToString(Ops.Call(Ops.GetDescriptor(ret, o, ut)));
            }

            return PythonType.ReprMethod(o).ToString();
        }

        #endregion

        #region Overloaded Unary/Binary operators

        public override object Negate(object self) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpNegate, out func)) return Ops.Call(func);

            return Ops.NotImplemented;
        }

        public override object OnesComplement(object self) {
            object func;
            if (Ops.TryGetAttr(self, SymbolTable.OpOnesComplement, out func)) return Ops.Call(func);
            return Ops.NotImplemented;
        }

        public override object CompareTo(object self, object other) {
            object func;
            if (PythonType.TryLookupSpecialMethod(self, SymbolTable.Cmp, out func)) {
                return Ops.Call(func, other);
            }

            return Ops.NotImplemented;
        }

        public override object Equal(object self, object other) {
            object func;
            if (PythonType.TryLookupSpecialMethod(self, SymbolTable.OpEqual, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }

            if (PythonType.TryLookupSpecialMethod(self, SymbolTable.Cmp, out func) && func != __cmp__F) {
                object ret = Ops.Call(func, other);
                if (ret != Ops.NotImplemented) return Ops.CompareToZero(ret) == 0;

                //if (ret is int) {
                //    return ((int)ret) == 0;
                //} else if (ret is ExtensibleInt) {
                //    return ((ExtensibleInt)ret)._value == 0;
                //}
                //throw Ops.TypeError("comparison did not return an int");
            }

            return Ops.NotImplemented;
        }

        public override object NotEqual(object self, object other) {
            object func;
            if (PythonType.TryLookupSpecialMethod(self, SymbolTable.OpNotEqual, out func)) {
                object ret;
                if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
            }

            if (PythonType.TryLookupSpecialMethod(self, SymbolTable.Cmp, out func) && func != __cmp__F) {
                object ret = Ops.Call(func, other);
                if (ret != Ops.NotImplemented) return Ops.CompareToZero(ret) != 0;

                //if (ret is int) {
                //    return ((int)ret) != 0;
                //} else if (ret is ExtensibleInt) {
                //    return ((ExtensibleInt)ret)._value != 0;
                //}
                //throw Ops.TypeError("comparison did not return an int");
            }

            return Ops.NotImplemented;
        }

        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            object res;
            if (dict.TryGetValue(SymbolTable.WeakRef, out res)) {
                return res as WeakRefTracker;
            }
            return null;
        }

        void IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            dict[SymbolTable.WeakRef] = value;
        }
        #endregion

        #region WeakRef helpers

        public static void SetWeakRefHelper(object self, WeakRefTracker value) {
            ISuperDynamicObject sdo = self as ISuperDynamicObject;
            System.Diagnostics.Debug.Assert(sdo != null);

            IAttributesDictionary d = GetInitializedDict(sdo);
            d[SymbolTable.WeakRef] = value;
        }

        public static WeakRefTracker GetWeakRefHelper(object self) {
            ISuperDynamicObject sdo = self as ISuperDynamicObject;
            if (sdo != null) {
                IAttributesDictionary dict = sdo.GetDict();
                if (dict != null) {
                    object res;
                    if (dict.TryGetValue(SymbolTable.WeakRef, out res)) {
                        return res as WeakRefTracker;
                    }
                }
            }
            return null;
        }
        #endregion
    }

    public class BigNamespaceDictionary : NamespaceDictionary {
        int[] nonInheritMap;
        public BigNamespaceDictionary(SymbolId[] knownKeys, Tuple bases)
            : base(knownKeys, bases) {
        }

        protected override void SortKeys() {
            int[] sortMap = new int[keys.Length];
            for (int i = 0; i < sortMap.Length; i++) sortMap[i] = i;
            Array.Sort(keys, sortMap);
            // we want the inverse of the map we got
            nonInheritMap = new int[sortMap.Length];
            for (int i = 0; i < sortMap.Length; i++) {
                nonInheritMap[sortMap[i]] = i;
            }
        }

        public override bool TryGetExtraValue(SymbolId key, out object value) {
            int index = Array.BinarySearch(keys, key);
            if (index >= 0) {
                value = values[index];
                return !(value is Uninitialized) && !isInherited[index]; //isInherited???
            }
            value = null;
            return false;
        }

        public override bool TrySetExtraValue(SymbolId key, object value) {
            int index = Array.BinarySearch(keys, key);
            if (index >= 0) {
                values[index] = value;
                isInherited[index] = false;
                return true;
            }
            value = null;
            return false;
        }

        public override bool TryGetNonInheritedValue(int key, out object value) {
            return base.TryGetNonInheritedValue(nonInheritMap[key], out value);
        }
    }

    public class NamespaceDictionary : CustomFieldIdDict, ICloneable {
        const int BinarySearchSize = 32;

        internal new SymbolId[] keys;
        internal new object[] values;
        internal bool[] isInherited;     // inheritance directly from CLS type, not from another UserType.
        internal Tuple bases;

        public static NamespaceDictionary Make(SymbolId[] knownKeys, Tuple bases) {
            if (knownKeys.Length > BinarySearchSize) {
                return new BigNamespaceDictionary(knownKeys, bases);
            } else {
                return new NamespaceDictionary(knownKeys, bases);
            }
        }

        protected NamespaceDictionary(SymbolId[] knownKeys, Tuple bases)
            : this(knownKeys) {
            this.bases = bases;
            for (int i = 0; i < bases.Count; i++) {
                UserType ut = bases[i] as UserType;
                if (ut != null) {
                    NamespaceDictionary nd = ut.dict as NamespaceDictionary;
                    if (nd != null) {
                        PropagateKeys(nd);
                    }
                }
            }
        }

        private NamespaceDictionary(SymbolId[] knownKeys)
            : base() {
            this.keys = knownKeys;

            SortKeys();

            int N = knownKeys.Length;
            this.values = new object[N];
            this.isInherited = new bool[N];
            for (int i = 0; i < N; i++) isInherited[i] = true;
        }

        public override SymbolId[] GetExtraKeys() {
            int count = 0;
            for (int i = 0; i < values.Length; i++) {
                if (!isInherited[i] && !(values[i] is Uninitialized)) count++;
            }
            SymbolId[] ret = new SymbolId[count];
            count = 0;
            for (int i = 0; i < values.Length; i++) {
                if (!isInherited[i] && !(values[i] is Uninitialized)) ret[count++] = keys[i];
            }
            return ret;
        }

        public override bool TrySetExtraValue(SymbolId key, object value) {
            SymbolId[] ks = keys;
            for (int i = 0; i < ks.Length; i++) {
                if (ks[i].Equals(key)) {
                    values[i] = value;
                    isInherited[i] = false;
                    return true;
                }
            }
            return false;
        }

        public override bool TryGetExtraValue(SymbolId key, out object value) {
            SymbolId[] ks = keys;
            for (int i = 0; i < ks.Length; i++) {
                if (ks[i].Equals(key)) {
                    value = values[i];
                    return !(value is Uninitialized) && !isInherited[i];
                }
            }
            value = null;
            return false;
        }

        /// <summary> Called from generated code for base class call </summary>
        public virtual bool TryGetNonInheritedValue(int key, out object value) {
            if (isInherited[key]) {
                value = null;
                return false;
            } else {
                value = values[key];
                return !(value is Uninitialized);
            }
        }

        protected virtual void SortKeys() {
        }

        private void PropagateKeys(NamespaceDictionary nd) {
            for (int i = 0; i < keys.Length && i < nd.keys.Length; i++) {
                if (nd.keys[i] == keys[i]) {
                    // common case: our layout matches our parents
                    PropagateInheritedKey(i, i, nd);
                } else {
                    // uncommon case - multiple inheritance only (maybe?)
                    for (int j = 0; j < keys.Length; j++) {
                        if (nd.keys[i] == keys[j]) {
                            PropagateInheritedKey(j, i, nd);
                        }
                    }
                }
            }
        }

        private void PropagateInheritedKey(int from, int to, NamespaceDictionary parent) {
            if (!parent.isInherited[from] && isInherited[to]) {
                // our parent didn't has an override, so we get
                // their override too.  Both are user defined functions
                // so we clear isInherited for this slot.
                values[to] = parent.values[to];
                isInherited[to] = false;
            }
        }
    }
}
