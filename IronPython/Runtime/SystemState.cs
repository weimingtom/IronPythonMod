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

using System.Diagnostics;
using System.Reflection;
using System.Text;

using System.Threading;

using IronPython.Compiler;

using ClrModule = IronPython.Modules.ClrModule;

namespace IronPython.Runtime {

    [PythonType(typeof(PythonModule))]
    public class SystemState : ICustomAttributes {
        internal Encoding DefaultEncoding;

        private IAttributesDictionary __dict__;
        private object exception_type, exception_value, exception_traceback;
        private TopReflectedPackage topPackage;
        private ClrModule clrModule;

        public SystemState() {
#if DEBUG
            // All fields should be initialized in Initialize()
            FieldInfo[] fields = typeof(SystemState).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo field in fields) {
                Debug.Assert(field.GetValue(this) == null || ((int)field.GetValue(this)) == 0);
            }
#endif
            Initialize();
        }

        /// <summary>
        /// Performs sys's initialization
        /// It is in it's own function so we can do reload(sys). On reload(sys), most of the attributes need to be
        /// reset. The following are left as they are - argv, exc_type, modules, path, path_hooks, path_importer_cache, ps1, ps2.
        /// </summary>
        public void Initialize() {
            if (__dict__ == null) {
                __dict__ = new FieldIdDict();

                // These fields do not get reset on "reload(sys)"
                argv = Ops.MakeList();
                modules = new Dict();
                path = List.Make();
                ps1 = Ops.ToPython(">>> ");
                ps2 = Ops.ToPython("... ");
                __stdin__ = new PythonFile(Console.OpenStandardInput(),
                                            Console.InputEncoding, 
                                            "<stdin>", 
                                            "r");
                __stdout__ = new PythonFile(Options.UnbufferedStdOutAndError ? Console.OpenStandardOutput(0) : Console.OpenStandardOutput(),
                                            Console.OutputEncoding, 
                                            "<stdout>", 
                                            "w");
                __stderr__ = new PythonFile(Options.UnbufferedStdOutAndError ? Console.OpenStandardError(0) : Console.OpenStandardError(), 
                                            Console.OutputEncoding, 
                                            "<stderr>", 
                                            "w");
            }

            __dict__[SymbolTable.Name] = "sys";            

            stdin = __stdin__;
            stdout = __stdout__;
            stderr = __stderr__;

            // removed from dictionary after the first call to set it.
            MethodInfo mi = typeof(SystemState).GetMethod("setdefaultencodingImpl", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            BuiltinMethodDescriptor descr = (BuiltinMethodDescriptor)new ReflectedMethod("setdefaultencoding", mi, FunctionType.PythonVisible | FunctionType.Method).GetDescriptor();

            __dict__[SymbolTable.SetDefaultEncoding] = descr.GetAttribute(this, this);

            DefaultEncoding = Encoding.ASCII;
            byteorder = BitConverter.IsLittleEndian ? "little" : "big";
            copyright = "Copyright (c) Microsoft Corporation. All rights reserved.";
            hexversion = 0x02040000;
            maxint = Int32.MaxValue;
            maxunicode = (int)ushort.MaxValue;
            platform = Ops.ToPython("cli");
            version_info = Tuple.MakeTuple(2, 4);
            // !!! These fields do need to be reset on "reload(sys)". However, the initial value is specified by the 
            // engine elsewhere. For now, we initialize them just once to some default value
            if (version == null) {
                version = IronPython.Hosting.PythonEngine.VersionString;
                warnoptions = List.Make();
                executable = "";
            }
        }

        public override string ToString() {
            return "<module 'sys' (built-in)>";
        }

        [PythonName("argv")]
        public object argv;

        public string byteorder;

        [PythonName("builtin_module_names")]
        public Tuple builtin_module_names;

        [PythonName("copyright")]
        public string copyright;

        [PythonName("warnoptions")]
        public List warnoptions;

        // as of 1.5 preferred access is exc_info, these may be null.
        public object exc_type {
            [PythonName("exc_type")]
            get {
                return exception_type;
            }
            set {
                exception_type = value;
            }
        }

        public object exc_value {
            [PythonName("exc_value")]
            get {
                return exception_value;
            }
            set {
                exception_value = value;
            }
        }

        public object exc_traceback {
            [PythonName("exc_traceback")]
            get {
                return exception_traceback;
            }
            set {
                exception_traceback = value;
            }
        }

        [ThreadStatic]
        internal static Exception RawException;

        [ThreadStatic]
        internal static TraceBack RawTraceBack;

        internal void SetRawException(Exception raw) {
            RawException = raw;

            // force update of non-thread static exception info...
            exc_info();
        }

        internal void ClearException() {
            SystemState.RawException = null;
            SystemState.RawTraceBack = null;
        }

        [PythonName("exc_info")]
        public Tuple exc_info() {
            if (RawException == null)  return Tuple.MakeTuple(null, null, null);
            object pyExcep = ExceptionConverter.ToPython(RawException);

            if(Options.TracebackSupport && RawTraceBack != null){
                RawTraceBack.UpdateFromStackTrace(new System.Diagnostics.StackTrace(RawException, true));
            }

            exc_traceback = RawTraceBack;

            if (pyExcep is StringException) {
                // string exceptions are special...  there tuple looks
                // like string, argument, traceback instead of
                //      type,   instance, traceback
                StringException se = RawException as StringException;
                Debug.Assert(se != null);

                exc_type = pyExcep;
                exc_value = se.Value;
                
                return Ops.MakeTuple(
                    pyExcep,
                    se.Value,
                    RawTraceBack);
            } else {
                object excType = Ops.GetAttr(DefaultContext.Default, pyExcep, SymbolTable.Class);
                exc_type = excType;
                exc_value = pyExcep;

                return Ops.MakeTuple(
                    excType,
                    pyExcep,
                    RawTraceBack);
            }            
        }

        [PythonName("exec_prefix")]
        public string exec_prefix;
        [PythonName("executable")]
        public object executable;

        [PythonName("exit")]
        public void exit() {
            throw ExceptionConverter.CreateThrowable(ExceptionConverter.GetPythonExceptionByName("SystemExit"));
        }

        [PythonName("exit")]
        public void exit(object code) {
            // throw as a python exception here to get the args set.
            throw ExceptionConverter.CreateThrowable(ExceptionConverter.GetPythonExceptionByName("SystemExit"), code);
        }

        [PythonName("getdefaultencoding")]
        public string getdefaultencoding() {
            return DefaultEncoding.WebName.ToLower().Replace('-','_');
        }

        [PythonName("getfilesystemencoding")]
        public object getfilesystemencoding() {
            return null;
        }

        [PythonName("_getframe")]
        public object _getframe() {
            throw Ops.ValueError("_getframe is not implemented");
        }

        [PythonName("_getframe")]
        public object _getframe(int depth) {
            throw Ops.ValueError("_getframe is not implemented");
        }

        [PythonName("hexversion")]
        public int hexversion;

        [PythonName("maxint")]
        public int maxint;
        [PythonName("maxunicode")]
        public object maxunicode;

        [PythonName("modules")]
        public IDictionary<object, object> modules;

        [PythonName("path")]
        public List path;

        [PythonName("platform")]
        public object platform;

        [PythonName("prefix")]
        public string prefix;

        [PythonName("ps1")]
        public object ps1;
        [PythonName("ps2")]
        public object ps2;

        private object setdefaultencodingImpl(object name) {
            if(name == null) throw Ops.TypeError("name cannot be None");
            string strName = name as string;
            if (strName == null) throw Ops.TypeError("name must be a string");

            Encoding enc;
            if(!StringOps.TryGetEncoding(this, strName, out enc)){
                throw Ops.LookupError("'{0}' does not match any available encodings", strName);
            }

            DefaultEncoding = enc;
            __dict__.Remove(SymbolTable.SetDefaultEncoding); 
            return null;
        }
        
        [PythonName("setrecursionlimit")]
        public void SetRecursionLimit(int limit) {
            if (limit < 0) throw Ops.ValueError("recursion limit must be positive");
            PythonFunction.EnforceRecursion = (limit != Int32.MaxValue);
            PythonFunction.MaximumDepth = limit;
        }

        [PythonName("getrecursionlimit")]
        public object GetRecursionLimit() {
            return Ops.Int2Object(PythonFunction.MaximumDepth);
        }

        [PythonName("stdin")]
        public object stdin;
        [PythonName("stdout")]
        public object stdout;
        [PythonName("stderr")]
        public object stderr;

        [PythonName("__stdin__")]
        public object __stdin__;
        [PythonName("__stdout__")]
        public object __stdout__;
        [PythonName("__stderr__")]
        public object __stderr__;

        [PythonName("version")]
        public string version;

        [PythonName("version_info")]
        public object version_info;

        #region ICustomAttributes Members

        public bool TryGetAttr(ICallerContext context, SymbolId name, out object value) {
            if (name == SymbolTable.Dict) {
                value = __dict__;
                return true;
            }

            if (TypeCache.SystemState.TryGetAttr(context, this, name, out value)) return true;
            return __dict__.TryGetValue(name, out value);
        }

        public void SetAttr(ICallerContext context, SymbolId name, object value) {
            object dummy;
            if (TypeCache.SystemState.TryGetAttr(context, this, name, out dummy)) {
                TypeCache.SystemState.SetAttr(context, this, name, value);
            } else {
                __dict__[name] = value;
            }
        }

        public void DeleteAttr(ICallerContext context, SymbolId name) {
            object dummy;
            if (TypeCache.SystemState.TryGetAttr(context, this, name, out dummy)) {
                try {
                    TypeCache.SystemState.SetAttr(context, this, name, new Uninitialized((string)SymbolTable.IdToString(name)));
                } catch {
                    throw new NotImplementedException("deleting from sys");
                }
            } else {
                __dict__.Remove(name);
            }
        }
                
        public List GetAttrNames(ICallerContext context) {
            List ret = new List(((IDictionary<object,object>)__dict__).Keys);
            ret.Extend(TypeCache.SystemState.GetAttrNames(context, this));
            return ret;
        }

        public IDictionary<object, object> GetAttrDict(ICallerContext context) {
            Dict res = new Dict(__dict__);
            Dict append = TypeCache.SystemState.GetAttrDict(context, this);
            foreach (KeyValuePair<object, object> pair in append) {
                res.Add(pair);
            }
            return res;
        }

        #endregion

        [PythonName("LoadAssemblyByName")]
        public void LoadAssemblyByName(string name) {
            throw Ops.Warning("sys.LoadAssemblyByName has been deprecated and will be removed in the next release. Please use clr.AddReference* methods instead.");
        }

        [PythonName("LoadAssemblyFromFile")]
        public void LoadAssemblyFromFile(string filename) {
            throw Ops.Warning("sys.LoadAssemblyFromFile has been deprecated and will be removed in the next release. Please use clr.AddReference* methods instead.");
        }        

        internal TopReflectedPackage TopPackage {
            get {
                if (topPackage == null) 
                    Interlocked.CompareExchange<TopReflectedPackage>(ref topPackage, new TopReflectedPackage(), null);
                
                return topPackage;
            }
        }

        internal ClrModule ClrModule {
            get {
                if (clrModule == null) {
                    ClrModule mod = new ClrModule(this);
                    if (Interlocked.CompareExchange<ClrModule>(ref clrModule, mod, null) != null) {
                        mod.Dispose();
                    }
                }
                return clrModule;
            }
        }
    }
}
