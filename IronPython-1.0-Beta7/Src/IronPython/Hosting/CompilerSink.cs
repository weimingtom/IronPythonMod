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
using System.Collections.Generic;
using System.Text;

namespace IronPython.Hosting {
    public enum Severity {
        Message,
        Warning,
        Error,
    }

    public static class ErrorCodes {
        public const int SyntaxError = 2;
        public const int IndentationError = 3;
        public const int TabError = 4;
    }

    public struct CodeSpan {
        public int startLine;
        public int startColumn;
        public int endLine;
        public int endColumn;

        public CodeSpan(int startLine, int startColumn, int endLine, int endColumn) {
            this.startLine = startLine;
            this.startColumn = startColumn;
            this.endLine = endLine;
            this.endColumn = endColumn;
        }

        public CodeSpan(IronPython.Compiler.Location start, IronPython.Compiler.Location end) {
            this.startLine = start.line;
            this.startColumn = start.column;
            this.endLine = end.line;
            this.endColumn = end.column;
        }
    }

    public abstract class CompilerSink {
        //!!! way too many parameters...
        public abstract void AddError(string path, string message, string lineText,
                                      int startLine, int startColumn,
                                      int endLine, int endColumn,
                                      int errorCode, Severity severity);

        public void AddError(string path, string message, int errorCode, Severity severity) {
            AddError(path, message, String.Empty, 0, 0, 0, 0, errorCode, severity);
        }

        public virtual void AddError(string path, string message, CodeSpan location, int errorCode, Severity severity) {
            AddError(path, message, String.Empty, location.startLine, location.startColumn, location.endLine, location.endColumn, errorCode, severity);
        }

        public virtual void MatchPair(CodeSpan start, CodeSpan end, int priority) {
        }

        public virtual void MatchTriple(CodeSpan start, CodeSpan middle, CodeSpan end, int priority) {
        }

        public virtual void EndParameters(CodeSpan span) {
        }

        public virtual void NextParameter(CodeSpan span) {
        }

        public virtual void QualifyName(CodeSpan selector, CodeSpan span, string name) {
        }

        public virtual void StartName(CodeSpan span, string name) {
        }

        public virtual void StartParameters(CodeSpan context) {
        }
    }

    public class CompilerExceptionSink : CompilerSink {
        public override void AddError(string path, string message, string lineText, int startLine, int startColumn, int endLine, int endColumn, int errorCode, Severity severity) {
            string sev;
            if (severity >= Severity.Error)
                sev = "Error";
            else if (severity >= Severity.Warning)
                sev = "Warning";
            else
                sev = "Message";

            throw new Exception(string.Format("{0}:{1} at {2} {3}:{4}-{5}:{6}", sev, message, path, startLine, startColumn, endLine, endColumn));
        }
    }
}
