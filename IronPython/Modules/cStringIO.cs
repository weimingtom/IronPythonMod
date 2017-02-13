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
using System.IO;
using System.Text;
using IronPython.Runtime;

[assembly: PythonModule("cStringIO", typeof(IronPython.Modules.PythonStringIO))]
namespace IronPython.Modules {
    // place holder for cStringIO module so tests that import but don't use it work

    public class StringStream {
        private string data;
        private int position;
        private int length;

        public StringStream(string data) {
            this.data = data;
            this.position = 0;
            this.length = data == null ? 0 : data.Length;
        }

        public string Data {
            get {
                return data;
            }
            set {
                data = value;
                if (data == null) {
                    length = position = 0;
                } else {
                    length = data.Length;
                    if (position > length) {
                        position = length;
                    }
                }
            }
        }

        public string Prefix {
            get {
                return data.Substring(0, position);
            }
        }

        public int Read() {
            if (data == null) {
                throw EndOfFile();
            }

            if (position < length) {
                return data[position++];
            } else {
                return -1;
            }
        }

        public string Read(int i) {
            if (data == null) {
                throw EndOfFile();
            }
            if (position + i > length) {
                i = length - position;
            }
            string ret = data.Substring(position, i);
            position += i;
            return ret;
        }

        public string ReadLine() {
            if (data == null) {
                throw EndOfFile();
            }

            int i = position;
            while (i < length) {
                char c = data[i];
                if (c == '\n' || c == '\r') {
                    i++;
                    if (c == '\r' && position < length && data[i] == '\n') {
                        i++;
                    }
                    // preserve newline character like StringIO
                    
                    string res = data.Substring(position, i - position);
                    position = i;
                    return res;
                }
                i++;
            }

            if (i > position) {
                string res = data.Substring(position, i - position);
                position = i;
                return res;
            }

            return null;
        }

        public int Seek(int offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    position = offset; break;
                case SeekOrigin.Current:
                    position = position + offset; break;
                case SeekOrigin.End:
                    position = length + offset; break;
                default:
                    throw new ArgumentException("origin");
            }
            return position;
        }

        private static Exception EndOfFile() {
            return Ops.EofError("end of file");
        }
    }

    public static class PythonStringIO {
        [PythonType("StringIO")]
        public class StringIO {
            private StringWriter sw = new StringWriter();
            private StringStream sr;

            public StringIO() {
                sr = null;
            }
            public StringIO(string s) {
                sr = new StringStream(s);
            }

            [PythonName("read")]
            public int Read() {
                if (sr == null) {
                    throw Ops.EofError("");
                }
                FixStreams();
                return sr.Read();
            }

            [PythonName("read")]
            public string Read(int i) {
                if (sr == null) {
                    throw Ops.EofError("");
                }
                FixStreams();
                return sr.Read(i);
            }

            [PythonName("readline")]
            public string ReadLine() {
                if (sr == null) {
                    throw Ops.EofError("");
                }
                FixStreams();
                return sr.ReadLine();
            }

            [PythonName("write")]
            public void Write(string s) {
                sw.Write(s);
            }

            [PythonName("seek")]
            public void Seek(int offset, int origin) {
                FixStreams();
                sr.Seek(offset, (SeekOrigin)origin);
            }

            [PythonName("getvalue")]
            public string GetValue() {
                if (sr != null) {
                    FixStreams();
                    return sr.Data;
                } else {
                    return sw.GetStringBuilder().ToString();
                }
            }

            private void FixStreams() {
                if (sr != null) {
                    StringBuilder sb = sw.GetStringBuilder();
                    if (sb != null && sb.Length > 0) {
                        sr.Data = sr.Prefix + sb.ToString();
                        sb.Length = 0;
                    }
                }
            }

            #region IEnumerator Members

            public object Current {
                get { throw new Exception("The method or operation is not implemented."); }
            }

            public bool MoveNext() {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Reset() {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion
        }
    }
}