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

using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Hosting;

namespace IronPython.Compiler {
    internal class SimpleParserSink : CompilerSink {
        public override void AddError(string path, string message, string lineText, int startLine, int startColumn, int endLine, int endColumn, int errorCode, Severity severity) {
            if (severity == Severity.Warning) {
                throw Ops.SyntaxWarning(message, path, startLine, startColumn, lineText, severity);
            }

            switch (errorCode) {
                case ErrorCodes.IndentationError:
                    throw Ops.IndentationError(message, path, startLine, startColumn, lineText, errorCode, severity);
                case ErrorCodes.TabError:
                    throw Ops.TabError(message, path, startLine, startColumn, lineText, errorCode, severity);
                default:
                    throw Ops.SyntaxError(message, path, startLine, startColumn, lineText, errorCode, severity);
            }
        }
    }
}
