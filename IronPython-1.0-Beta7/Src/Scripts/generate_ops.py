#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
#
#  You must not remove this notice, or any other, from this software.
#
######################################################################################

import generate
reload(generate)
from generate import CodeGenerator
import operator

kwlist = ['and', 'assert', 'break', 'class', 'continue', 'def', 'del', 'elif', 'else', 'except', 'exec', 'finally', 'for', 'from', 'global', 'if', 'import', 'in', 'is', 'lambda', 'not', 'or', 'pass', 'print', 'raise', 'return', 'try', 'while', 'yield']

class Symbol:
    def __init__(self, symbol, name):
        self.symbol = symbol
        self.name = name

    def cap_name(self):
        return self.name.title().replace(' ', '')

    def upper_name(self):
        return self.name.upper().replace(' ', '_')

    def title_name(self):
        return self.name.title().replace(' ', '')

    def simple_name(self):
        return self.name[0] + self.cap_name()[1:]

    def __repr__(self):
        return 'Symbol(%s)' % self.symbol

class Operator(Symbol):
    def __init__(self, symbol, name, rname, clrName=None,prec=-1, opposite=None, bool1=None, bool2=None, bool3=None):
        Symbol.__init__(self, symbol, name)
        self.rname = rname
        self.clrName = clrName
        self.meth_name = "__" + name + "__"
        self.rmeth_name = "__" + rname + "__"
        if name in kwlist:
            name = name + "_"
        #self.op = getattr(operator, name)
        self.prec = prec
        self.opposite = opposite
        self.bool1 = bool1
        self.bool2 = bool2
        self.bool3 = bool3

    def clrInPlaceName(self):
        return "InPlace" + self.clrName

    def title_name(self):
        return self.clrName

    def isCompare(self):
        return self.prec == -1

    def __repr__(self):
        return 'Operator(%s,%s,%s)' % (self.symbol, self.name, self.rname)

    def getCreator(self):
        if self.isCompare():
            return 'new BinaryOperator("%s", new CallTarget2(Ops.%s), null, -1)' % (self.symbol, self.clrName)
        else:
            return 'new BinaryOperator("%s", new CallTarget2(Ops.%s), new CallTarget2(Ops.%s), %d)' % (self.symbol, self.clrName, self.clrInPlaceName(), self.prec)

    def generate_binop(self, cw):
        extra = ""
        extra_early = ""
        extra_vars = ""

        #if self.symbol in ['//', '**']: template = UBINOP
        if self.isCompare(): template = CMPOP
        elif self.prec <= 3: template = IBINOP
        else: template = BINOP

        if self.symbol == '+': 
            extra = ADD_EXTRA
            extra_early = ADD_EXTRA_EARLY
            extra_vars=ADD_EXTRA_VARS
        elif self.symbol == '*': 
            extra = MUL_EXTRA

        if self.isCompare():
            cw.write(template, name=self.clrName, rname=self.rname, symbol=self.symbol, extra_code=extra, opposite=self.opposite, bool1=self.bool1, bool2=self.bool2, bool3=self.bool3, rettype="object", suffix="", return_object="ret", extra_early_code=extra_early, extra_vars=extra_vars, boolTransform='Ops.Bool2Object')
            cw.write(template, name=self.clrName, rname=self.rname, symbol=self.symbol, extra_code=extra, opposite=self.opposite, bool1=self.bool1, bool2=self.bool2, bool3=self.bool3, rettype="bool", suffix="RetBool", return_object="Ops.IsTrue(ret)", extra_early_code=extra_early, extra_vars=extra_vars, boolTransform='')
        else:
            cw.write(template, name=self.clrName, rname=self.rname, symbol=self.symbol, extra_code=extra, extra_early_code=extra_early, extra_vars=extra_vars)

        if template is BINOP:
            if self.clrInPlaceName() == 'InPlaceAdd':
                cw.write(INPLACE_OP, name = self.clrName, inname=self.clrInPlaceName(),symbol=self.symbol, extra_code=INPLACE_ADD_EXTRA, extra_early_code=extra_early, extra_vars=extra_vars)
            else:
                cw.write(INPLACE_OP, name = self.clrName, inname=self.clrInPlaceName(),symbol=self.symbol, extra_code='', extra_early_code=extra_early, extra_vars=extra_vars)
        elif template is IBINOP:
            cw.write(IINPLACE_OP, name = self.clrName, inname=self.clrInPlaceName(),symbol=self.symbol, extra_early_code=extra_early, extra_vars=extra_vars)
    
    def genDynamicTypeOp(self, cw, basicTemplate, cmpTemplate):
        if self.isCompare(): template = cmpTemplate
        else: template = basicTemplate
        cw.write(template, clrName=self.clrName, name=self.name, rname=self.rname, symbol=self.symbol)
    
    def genSymbolTableValues(self, cw, x):
        cw.writeline("public const int Op%sId = %d;" % (self.title_name(),x))

        if self.isCompare(): return 1

        cw.writeline("public const int OpReverse%sId = %d;" % (self.title_name(),x+1))
        cw.writeline("public const int OpInPlace%sId = %d;" % (self.title_name(),x+2))
        
        return 3
    
    def genSymbolTableAdd(self, cw, x):
        cw.writeline("StringToId(\"__%s__\");  // %d " % (self.name,x))
        if self.isCompare(): return 1
        
        cw.writeline("StringToId(\"__%s__\");  // %d " % (self.rname,x+1))
        cw.writeline("StringToId(\"__i%s__\");  // %d " % (self.name,x+2))
        
        return 3
        
    def genSymbolTableSymbols(self, cw, x):
        cw.writeline("public static readonly SymbolId Op%s = new SymbolId(Op%sId);" % (self.title_name(),self.title_name()))
        
        if self.isCompare(): return 1
        
        cw.writeline("public static readonly SymbolId OpReverse%s = new SymbolId(OpReverse%sId);" % (self.title_name(),self.title_name()))
        cw.writeline("public static readonly SymbolId OpInPlace%s = new SymbolId(OpInPlace%sId);" % (self.title_name(),self.title_name()))
        
        return 3

class DivisionOperator(Operator):
    def __init__(self, symbol, name, rname, clrName, prec, tname, trname, tclrName, opposite=None):
        Operator.__init__(self, symbol, name, rname, clrName, prec, opposite)
        self.tname = tname
        self.trname = trname
        self.tclrName = tclrName
        self.tmeth_name = "__" + tname + "__"
        self.trmeth_name = "__" + trname + "__"
        #self.op = getattr(operator, name)
        self.prec = prec
        self.opposite = opposite

    def clrInPlaceTName(self):
        return "InPlace" + self.tclrName

    def __repr__(self):
        return 'DivisionOperator(%s,%s,%s)' % (self.symbol, self.name, self.rname)

    def getCreator(self):
        return 'new DivisionOperator("%s", new CallTarget2(Ops.%s), new CallTarget2(Ops.%s), new CallTarget2(Ops.%s), new CallTarget2(Ops.%s), %d)' % (self.symbol, self.clrName, self.clrInPlaceName(), self.tclrName, self.clrInPlaceTName(), self.prec)

    def generate_binop(self, cw):
        template = BINOP
        cw.write(template, name=self.clrName, rname=self.rname, symbol=self.symbol, extra_code='', extra_early_code='', extra_vars='')
        cw.write(INPLACE_OP, name = self.clrName, inname=self.clrInPlaceName(),symbol=self.symbol, extra_code='', extra_early_code='', extra_vars='')

        cw.write(template, name=self.tclrName, rname=self.trname, symbol=self.symbol, extra_code='', extra_early_code='', extra_vars='')
        cw.write(INPLACE_OP, name = self.tclrName, inname=self.clrInPlaceTName(),symbol=self.symbol, extra_code='', extra_early_code='', extra_vars='')

    def genDynamicTypeOp(self, cw, basicTemplate, cmpTemplate):
        template = basicTemplate
        cw.write(template, clrName=self.clrName, name=self.name, rname=self.rname, symbol=self.symbol)
        cw.write(template, clrName=self.tclrName, name=self.tname, rname=self.trname, symbol=self.symbol)

    def genSymbolTableValues(self, cw, x):
        cw.writeline("public const int Op%sId = %d;" % (self.title_name(),x))
        cw.writeline("public const int OpReverse%sId = %d;" % (self.title_name(),x+1))
        cw.writeline("public const int OpInPlace%sId = %d;" % (self.title_name(),x+2))

        cw.writeline("public const int Op%sId = %d;" % (self.tclrName,x+3))
        cw.writeline("public const int OpReverse%sId = %d;" % (self.tclrName,x+4))
        cw.writeline("public const int OpInPlace%sId = %d;" % (self.tclrName,x+5))
        
        return 6
    
    def genSymbolTableAdd(self, cw, x):
        cw.writeline("StringToId(\"__%s__\");  // %d " % (self.name,x))
        cw.writeline("StringToId(\"__%s__\");  // %d " % (self.rname,x+1))
        cw.writeline("StringToId(\"__i%s__\");  // %d " % (self.name,x+2))

        cw.writeline("StringToId(\"__%s__\");  // %d " % (self.tname,x+3))
        cw.writeline("StringToId(\"__%s__\");  // %d " % (self.trname,x+4))
        cw.writeline("StringToId(\"__i%s__\");  // %d " % (self.tname,x+5))
        return 6
    
    def genSymbolTableSymbols(self, cw, x):
        cw.writeline("public static readonly SymbolId Op%s = new SymbolId(%d);" % (self.title_name(),x))
        cw.writeline("public static readonly SymbolId OpReverse%s = new SymbolId(%d);" % (self.title_name(),x+1))
        cw.writeline("public static readonly SymbolId OpInPlace%s = new SymbolId(%d);" % (self.title_name(),x+2))
        
        cw.writeline("public static readonly SymbolId Op%s = new SymbolId(%d);" % (self.tclrName,x+3))
        cw.writeline("public static readonly SymbolId OpReverse%s = new SymbolId(%d);" % (self.tclrName,x+4))
        cw.writeline("public static readonly SymbolId OpInPlace%s = new SymbolId(%d);" % (self.tclrName,x+5))
        
        return 6
class Grouping(Symbol):
    def __init__(self, symbol, name, side):
        Symbol.__init__(self, symbol, side+" "+name)
        self.base_name = name
        self.side = side

ops = []

"""
0 expr: xor_expr ('|' xor_expr)*
1 xor_expr: and_expr ('^' and_expr)*
2 and_expr: shift_expr ('&' shift_expr)*
3 shift_expr: arith_expr (('<<'|'>>') arith_expr)*
4 arith_expr: term (('+'|'-') term)*
5 term: factor (('*'|'/'|'%'|'//') factor)*
"""
binaries1 = [('+', 'add', 4, 'Add'), ('-', 'sub',4,'Subtract'), ('**', 'pow', 6,'Power'), ('*', 'mul', 5,'Multiply'),
            ('//', 'floordiv', 5, 'FloorDivide')]
binaries2 = [('%', 'mod', 5, 'Mod'), ('<<', 'lshift', 3, 'LeftShift'), ('>>', 'rshift', 3, 'RightShift'),
            ('&', 'and', 2, 'BitwiseAnd'), ('|', 'or', 0, 'BitwiseOr'), ('^', 'xor', 1, 'Xor')]

def add_binaries(list):
    for sym, name, prec,clrName in list:
        ops.append(Operator(sym, name, 'r'+name, clrName, prec))
        ops.append(Symbol(sym+"=", name+" equal"))

add_binaries(binaries1)

sym, name, prec, clrName, tname, tclrName = ('/', 'div', 5, 'Divide', 'truediv', 'TrueDivide')
ops.append(DivisionOperator(sym, name, 'r'+name, clrName, prec, tname, 'r'+tname, tclrName))
ops.append(Symbol(sym+"=", name+" equal"))

add_binaries(binaries2)

compares = [('<', 'lt', 'ge', 'LessThan', 'GreaterThan', "false", "true", "false"), 
            ('>', 'gt', 'le', 'GreaterThan', 'LessThan', "false", "false", "true"),
            ('<=', 'le', 'gt', 'LessThanOrEqual', 'GreaterThanOrEqual', "true", "true", "false"), 
            ('>=', 'ge', 'lt', 'GreaterThanOrEqual', 'LessThanOrEqual', "true", "false", "true"),
            ('==', 'eq', 'eq', 'Equal', 'NotEqual', 'a', 'a', 'a'), 
            ('!=', 'ne', 'ne', 'NotEqual', 'Equal', 'a', 'a', 'a'), 
            ('<>', 'lg', 'lg', 'LessThanGreaterThan', 'Equal', 'a', 'a', 'a')]
for sym, name, rname,clrName,opposite, bool1, bool2, bool3 in compares:
    ops.append(Operator(sym, name, rname,clrName, opposite=opposite, bool1=bool1, bool2=bool2, bool3=bool3))

groupings = [('(', ')', 'paren'), ('[', ']', 'bracket'), ('{', '}', 'brace')]
for sym, rsym, name in groupings:
    ops.append(Grouping(sym, name, 'l'))
    ops.append(Grouping(rsym, name, 'r'))

simple = [(',', 'comma'), (':', 'colon'), ('`', 'backquote'), (';', 'semicolon'),
          ('=', 'assign'), ('~', 'twidle'), ('@', 'at')]
for sym, name in simple:
    ops.append(Symbol(sym, name))

start_symbols = {}
for op in ops:
    ss = op.symbol[0]
    if not start_symbols.has_key(ss): start_symbols[ss] = []
    start_symbols[ss].append(op)


def gen_tests(ops, pos, indent=1):
    ret = []

    default_match = []
    future_matches = {}
    for sop in ops:
        if len(sop.symbol) == pos:
            default_match.append(sop)
        elif len(sop.symbol) > pos:
            ch = sop.symbol[pos]
            if future_matches.has_key(ch):
                future_matches[ch].append(sop)
            else:
                future_matches[ch] = [sop]
    assert len(default_match) <= 1
    for ch, sops in future_matches.items():
        ret.append("if (NextChar('%s')) {" % ch)
        ret.extend(gen_tests(sops, pos+1))
        ret.append("}")
    if default_match:
        op = default_match[0]
        if isinstance(op, Grouping):
            if op.side == 'l':
                ret.append("%sLevel++;" % op.base_name);
            else:
                ret.append("%sLevel--;" % op.base_name);
        ret.append("SetEnd(); return Tokens.%sToken;" %
                   op.title_name())
    else:
        ret.append("return BadChar(NextChar());")

    return ["    "*indent + l for l in ret]

def tokenize_generator(cw):
    ret = []
    done = {}
    for op in ops:
        ch = op.symbol[0]
        if done.has_key(ch): continue
        sops = start_symbols[ch]
        cw.write("case '%s':" % ch)
        for t in gen_tests(sops, 1):
            cw.write(t)
        done[ch] = True
    return ret


CodeGenerator("Tokenize Ops", tokenize_generator).doit()

def tokenkinds_generator(cw):
    i = 32
    for op in ops:
        cw.write("%s = %d," % (op.title_name(), i))
        i += 1

    cw.writeline()
    keyword_list = list(kwlist)
    keyword_list.sort()
    for kw in keyword_list:
        cw.write("Keyword%s = %d," % (kw.title(), i))
        i += 1

CodeGenerator("Token Kinds", tokenkinds_generator).doit()

def tokens_generator(cw):
    for op in ops:
        if isinstance(op, Operator) and op.name != "lg":
            creator = 'new OperatorToken(TokenKind.%s, Operator.%s)' % (
                op.title_name(), op.title_name())
        else:
            creator = 'new SymbolToken(TokenKind.%s, "%s")' % (
                op.title_name(), op.symbol)
        cw.write("public static readonly Token %sToken = %s;" %
                   (op.title_name(), creator))

    cw.writeline()
    keyword_list = list(kwlist)
    keyword_list.sort()

    dict_init = []

    for kw in keyword_list:
        creator = 'new SymbolToken(TokenKind.Keyword%s, "%s")' % (
            kw.title(), kw)
        cw.write("public static readonly Token Keyword%sToken = %s;" %
               (kw.title(), creator))

        dict_init.append("Keywords[Compiler.Name.Make(\"%s\")] = Keyword%sToken;" %
                         (kw, kw.title()))

    cw.writeline()
    cw.write("public static readonly Dictionary<Name, Token> Keywords = new Dictionary<Name, Token>();");
    cw.writeline()
    cw.enter_block("static Tokens()")
    for l in dict_init: cw.write(l)
    cw.exit_block()

CodeGenerator("Tokens", tokens_generator).doit()

def operators_generator(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        if op.name == "lg": continue

        creator = op.getCreator()
        cw.write("public static readonly BinaryOperator %s = %s;" % (op.title_name(), creator))

CodeGenerator("Operators", operators_generator).doit()

IBINOP = """
public static object %(name)s(object x, object y) {
    object ret;
    if (x is int) {
        ret = IntOps.%(name)s((int)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is IronMath.BigInteger) {
        ret = LongOps.%(name)s((IronMath.BigInteger)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is long) {
        ret = Int64Ops.%(name)s((long)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is bool) {
        ret = BoolOps.%(name)s((bool)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is byte) {
        ret = IntOps.%(name)s((int)(byte)x, y);
        if (ret != NotImplemented) return Int2ByteOrInt(ret);
    }
    ret = GetDynamicType(x).%(name)s(x, y);
    if (ret != NotImplemented) return ret;
    ret = GetDynamicType(y).Reverse%(name)s(y, x);
    if (ret != NotImplemented) return ret;

    IProxyObject po = x as IProxyObject;
    if (po != null) return %(name)s(po.Target, y);
    po = y as IProxyObject;
    if (po != null) return %(name)s(x, po.Target);

    throw Ops.TypeError("unsupported operand type(s) for %(symbol)s: '{0}' and '{1}'",
                        GetDynamicType(x).__name__, GetDynamicType(y).__name__);
}"""


BINOP = """
public static object %(name)s(object x, object y) {
    object ret;
%(extra_vars)s
    if (x is int) {
        ret = IntOps.%(name)s((int)x, y);
        if (ret != NotImplemented) return ret;
%(extra_early_code)s
    } else if (x is double) {
        ret = FloatOps.%(name)s((double)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is IronMath.Complex64) {
        ret = ComplexOps.%(name)s((IronMath.Complex64)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is IronMath.BigInteger) {
        ret = LongOps.%(name)s((IronMath.BigInteger)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is long) {
        ret = Int64Ops.%(name)s((long)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is float) {
        ret = FloatOps.%(name)s((float)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is ExtensibleFloat) {
        ret = FloatOps.%(name)s(((ExtensibleFloat)x).value, y);
        if (ret != NotImplemented) return ret;
    } else if (x is bool) {
        ret = BoolOps.%(name)s((bool)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is byte) {
        ret = IntOps.%(name)s((int)(byte)x, y);
        if (ret != NotImplemented) return Int2ByteOrInt(ret);
    }

%(extra_code)s

    ret = GetDynamicType(x).%(name)s(x, y);
    if (ret != NotImplemented) return ret;
    ret = GetDynamicType(y).Reverse%(name)s(y, x);
    if (ret != NotImplemented) return ret;

    IProxyObject po = x as IProxyObject;
    if (po != null) return %(name)s(po.Target, y);
    po = y as IProxyObject;
    if (po != null) return %(name)s(x, po.Target);

    throw Ops.TypeError("unsupported operand type(s) for %(symbol)s: '{0}' and '{1}'",
                        GetDynamicType(x).__name__, GetDynamicType(y).__name__);
}"""

CMPOP = """
public static %(rettype)s %(name)s%(suffix)s(object x, object y) {
    if (x is int) {
        if (y is int) {
            return %(boolTransform)s(((int)x) %(symbol)s ((int)y));
        } else if (y is double) {
            return %(boolTransform)s(((int)x) %(symbol)s ((double)y));
        } else if(y == null) {
            return %(boolTransform)s(1 %(symbol)s 0);
        } else {
            Conversion conv;
            double dbl = Converter.TryConvertToDouble(y, out conv);
            if(conv < Conversion.None) return %(boolTransform)s(((int)x) %(symbol)s dbl);            
        }
    } else if (x is double) {
        if (y is int) {
            return %(boolTransform)s(((double)x) %(symbol)s ((int)y));
        } else if (y is double) {
            return %(boolTransform)s(((double)x) %(symbol)s ((double)y));
        } else if(y is ExtensibleFloat) {
            return %(boolTransform)s(((double)x) %(symbol)s ((ExtensibleFloat)y).value);
        } else if(y == null) {
            return %(boolTransform)s(1 %(symbol)s 0);
        } else {
            Conversion conv;
            int val = Converter.TryConvertToInt32(y, out conv);
            if(conv < Conversion.None) return %(boolTransform)s(((double)x) %(symbol)s val);
        }
    } else if (x is bool) {
        if(y is bool) {
            return %(boolTransform)s((((bool)x)? 1 : 0) %(symbol)s (((bool)y)? 1 : 0));
        } else if(y == null) {
            return %(boolTransform)s(1 %(symbol)s 0);
        } else { 
            Conversion conv;
            int other = Converter.TryConvertToInt32(y, out conv);
            if(conv < Conversion.None) {
                if((bool)x)
                    return %(boolTransform)s(1 %(symbol)s (other));
                else
                    return %(boolTransform)s(0 %(symbol)s (other));
            }
        }
    } else if(x is BigInteger) {
        if(y is BigInteger) {
            return %(boolTransform)s(((BigInteger)x) %(symbol)s ((BigInteger)y));
        } else if(y is bool) {
            return %(boolTransform)s(((BigInteger)x) %(symbol)s (((bool)y) ? 1 : 0));
        } else if(y == null) {
            return %(boolTransform)s(1 %(symbol)s 0);
        }
    } else if(x is short) {
        object res = IntOps.Compare((int)(short)x, y);
        if(res != Ops.NotImplemented) return %(boolTransform)s(((int)res) %(symbol)s 0);
    } else if(x is ushort) {
        object res = IntOps.Compare((int)(ushort)x, y);
        if(res != Ops.NotImplemented) return %(boolTransform)s(((int)res) %(symbol)s 0);
    } else if(x is byte) {
        object res = IntOps.Compare((int)(byte)x, y);
        if(res != Ops.NotImplemented) return %(boolTransform)s(((int)res) %(symbol)s 0);
    } else if(x is sbyte) {
        object res = IntOps.Compare((int)(sbyte)x, y);
        if(res != Ops.NotImplemented) return %(boolTransform)s(((int)res) %(symbol)s 0);
    } else if(x is ulong) {
        object res = Int64Ops.Compare((ulong)x, y);
        if(res != Ops.NotImplemented) return %(boolTransform)s(((int)res) %(symbol)s 0);
    } else if(x is uint) {
        object res = Int64Ops.Compare((long)(uint)x, y);
        if(res != Ops.NotImplemented) return %(boolTransform)s(((int)res) %(symbol)s 0);
    } else if(x is decimal) {
        object res = FloatOps.Compare((double)(decimal)x, y);
        if(res != Ops.NotImplemented) return %(boolTransform)s(((int)res) %(symbol)s 0);
    } else if (x == null) {
        if(y == null) return %(boolTransform)s(%(bool1)s);
        
        if (y.GetType().IsPrimitive || y is BigInteger) {
            // built-in type that doesn't implement our comparable
            // interfaces, being compared against null, go ahead
            // and skip the rest of the checks.
            return %(boolTransform)s(0 %(symbol)s 1);
        }
    }
    
    if (x is string && y is string) {
        return %(boolTransform)s(string.CompareOrdinal((string)x, (string)y) %(symbol)s 0);
    }

    object ret;
    IRichComparable pc1 = x as IRichComparable;
    IRichComparable pc2 = y as IRichComparable;
    if (pc1 != null)
        if ((ret = pc1.%(name)s(y)) != Ops.NotImplemented) return %(return_object)s;
    if (pc2 != null)
        if ((ret = pc2.%(opposite)s(x)) != Ops.NotImplemented) return %(return_object)s;
    if (pc1 != null)
        if ((ret = pc1.CompareTo(y)) != Ops.NotImplemented) return %(boolTransform)s(Ops.CompareToZero(ret) %(symbol)s 0);
    if (pc2 != null)
        if ((ret = pc2.CompareTo(x)) != Ops.NotImplemented) return %(boolTransform)s((-1 * Ops.CompareToZero(ret)) %(symbol)s 0);
    
    Type xType = (x == null) ? null : x.GetType(), yType = (y == null) ? null : y.GetType();

    IComparable c = x as IComparable;
    if (c != null) {
        if (xType != null && xType != yType) {
            object z;
            try {
                Conversion conversion;
                z = Converter.TryConvert(y, xType, out conversion);
                if (conversion < Conversion.NonStandard) {
                    return %(boolTransform)s(c.CompareTo(z) %(symbol)s 0);
                }
            } catch {
            }
        } else {
            return %(boolTransform)s(c.CompareTo(y) %(symbol)s 0);
        }
    }
    c = y as IComparable;
    if (c != null) {
        if (yType != null && xType != yType) {
            try {
                Conversion conversion;
                object z = Converter.TryConvert(x, yType, out conversion);
                if (conversion < Conversion.NonStandard) {
                    return %(boolTransform)s(-1 * c.CompareTo(z) %(symbol)s 0);
                }
            } catch {
            }
        } else {
            return %(boolTransform)s(-1 * c.CompareTo(x) %(symbol)s 0);
        }
    }
    
    DynamicType dt1 = GetDynamicType(x);
    DynamicType dt2 = GetDynamicType(y);
    if ((ret = dt1.%(name)s(x, y)) != NotImplemented) return %(return_object)s;
    if ((ret = dt2.%(opposite)s(y, x)) != NotImplemented) return %(return_object)s;
    if ((ret = dt1.CompareTo(x, y)) != Ops.NotImplemented) return %(boolTransform)s(Ops.CompareToZero(ret) %(symbol)s 0);
    if ((ret = dt2.CompareTo(y, x)) != Ops.NotImplemented) return %(boolTransform)s((-1 * Ops.CompareToZero(ret)) %(symbol)s 0);

    if (xType == yType) {
        return %(boolTransform)s((IdDispenser.GetId(x) - IdDispenser.GetId(y)) %(symbol)s 0);
    } else {
        string xName = (xType == null) ? "!NoneType" : xType.Name, yName = (yType == null) ? "!NoneType" : yType.Name;
        
        return %(boolTransform)s(string.CompareOrdinal(xName, yName) %(symbol)s 0);
    }
}"""

INPLACE_OP = """
public static object %(inname)s(object x, object y) {
    object ret;
    if (x is int) {
        ret = IntOps.%(name)s((int)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is double) {
        ret = FloatOps.%(name)s((double)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is long) {
        ret = Int64Ops.%(name)s((long)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is IronMath.BigInteger) {
        ret = LongOps.%(name)s((IronMath.BigInteger)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is IronMath.Complex64) {
        ret = ComplexOps.%(name)s((Complex64)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is float) {
        ret = FloatOps.%(name)s((float)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is ExtensibleFloat) {
        ret = FloatOps.%(name)s(((ExtensibleFloat)x).value, y);
        if (ret != NotImplemented) return ret;
    } else if (x is bool) {
        ret = BoolOps.%(name)s((bool)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is byte) {
        ret = IntOps.%(name)s((int)(byte)x, y);
        if (ret != NotImplemented) return Int2ByteOrInt(ret);
    }

%(extra_code)s

    DynamicType dt = GetDynamicType(x);
    ret = dt.%(inname)s(x, y);
    if (ret != NotImplemented) return ret;
    ret = dt.%(name)s(x, y);
    if (ret != NotImplemented) return ret;

    IProxyObject po = x as IProxyObject;
    if (po != null) return %(inname)s(po.Target, y);
    po = y as IProxyObject;
    if (po != null) return %(inname)s(x, po.Target);

    throw Ops.TypeError("unsupported operand type(s) for %(symbol)s: '{0}' and '{1}'",
                        GetDynamicType(x).__name__, GetDynamicType(y).__name__);
}
"""
IINPLACE_OP = """
public static object %(inname)s(object x, object y) {
    object ret;
    if (x is int) {
        ret = IntOps.%(name)s((int)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is long) {
        ret = Int64Ops.%(name)s((long)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is IronMath.BigInteger) {
        ret = LongOps.%(name)s((IronMath.BigInteger)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is bool) {
        ret = BoolOps.%(name)s((bool)x, y);
        if (ret != NotImplemented) return ret;
    } else if (x is byte) {
        ret = IntOps.%(name)s((int)(byte)x, y);
        if (ret != NotImplemented) return Int2ByteOrInt(ret);
    }

    ret = GetDynamicType(x).%(inname)s(x, y);
    if (ret != NotImplemented) return ret;

    IProxyObject po = x as IProxyObject;
    if (po != null) return %(inname)s(po.Target, y);
    po = y as IProxyObject;
    if (po != null) return %(inname)s(x, po.Target);

    throw Ops.TypeError("unsupported operand type(s) for %(symbol)s: '{0}' and '{1}'",
                        GetDynamicType(x).__name__, GetDynamicType(y).__name__);
}
"""

UBINOP = """
public static object %(name)s(object x, object y) {
    throw new NotImplementedException("%(name)s");
}
"""
ADD_EXTRA_VARS = """    string sx,sy;
"""

ADD_EXTRA_EARLY = """    } else if((sx = x as string)!= null && (sy = y as string)!=null) {
        return sx + sy;"""

ADD_EXTRA = """                
    ISequence seq = x as ISequence;
    if (seq != null) { return seq.AddSequence(y); }
"""

INPLACE_ADD_EXTRA = """
    if (x is string && y is string) {
        return ((string)x) + ((string)y);
    }

    if (x is ReflectedEvent) {
        return ((ReflectedEvent)x).__iadd__(y);
    }
"""


MUL_EXTRA = """
    if (x is ISequence && y is int) {
        return ((ISequence)x).MultiplySequence((int)y);
    } else if (y is ISequence && x is int) {
        return ((ISequence)y).MultiplySequence((int)x);
    }
"""


def ops_generator(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        if op.symbol in ['==', '!=', "<>"]: continue
        op.generate_binop(cw)

CodeGenerator("Binary Ops", ops_generator).doit()


VIRTS = """
public virtual object %(clrName)s(object self, object other) {
    return Ops.NotImplemented;
}
public virtual object Reverse%(clrName)s(object self, object other) {
    return Ops.NotImplemented;
}
public virtual object InPlace%(clrName)s(object self, object other) {
    return Ops.%(clrName)s(self, other);
}"""

VIRT_CMP = """
public virtual object %(clrName)s(object self, object other) {
    return Ops.NotImplemented;
}"""

def ops_generator(cw, basicTemplate, cmpTemplate):
    for op in ops:
        if not isinstance(op, Operator): continue
        if op.name == "lg": continue
        op.genDynamicTypeOp(cw, basicTemplate, cmpTemplate)

def dyn_ops(cw):
    return ops_generator(cw, VIRTS, VIRT_CMP)

CodeGenerator("DynamicType Binary Ops", dyn_ops).doit()

def gen_SymbolTable_ops_values(cw):
    x = 0
    for op in ops:
        if not isinstance(op, Operator): continue
        
        x = x + op.genSymbolTableValues(cw, x)
        
def gen_SymbolTable_ops_added(cw):
    x = 0
    for op in ops:
        if not isinstance(op, Operator): continue
        
        x = x + op.genSymbolTableAdd(cw, x)  
        
def gen_SymbolTable_ops_symbols(cw):
    x = 0
    for op in ops:
        if not isinstance(op, Operator): continue
        
        x = x + op.genSymbolTableSymbols(cw, x)                
        
CodeGenerator("SymbolTable Ops Values", gen_SymbolTable_ops_values).doit()
CodeGenerator("SymbolTable Ops Added", gen_SymbolTable_ops_added).doit()        
CodeGenerator("SymbolTable Ops Symbols", gen_SymbolTable_ops_symbols).doit()        

OLDS = """
public override object %(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.Op%(clrName)s, out func)) return Ops.Call(func, other);
    return Ops.NotImplemented;
}
public override object Reverse%(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.OpReverse%(clrName)s, out func)) return Ops.Call(func, other);
    return Ops.NotImplemented;

}
public override object InPlace%(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.OpInPlace%(clrName)s, out func)) return Ops.Call(func, other);
    return Ops.NotImplemented;
}"""

OLD_CMP = """
public override object %(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.Op%(clrName)s, out func)) return Ops.Call(func, other);
    return Ops.NotImplemented;
}"""

def old_ops(cw):
    return ops_generator(cw, OLDS, OLD_CMP)

CodeGenerator("OldClass Binary Ops", old_ops).doit()


REFLECTED = """
public override object %(clrName)s(object self, object other) {
    object func, res;
    if (Ops.TryGetAttr(self, SymbolTable.Op%(clrName)s, out func) && Ops.TryCall(func, other, out res)) return res;
    return Ops.NotImplemented;
}
public override object Reverse%(clrName)s(object self, object other) {
    object func, res;
    if (Ops.TryGetAttr(self, SymbolTable.OpReverse%(clrName)s, out func) && Ops.TryCall(func, other, out res)) return res;
    return Ops.NotImplemented;

}
public override object InPlace%(clrName)s(object self, object other) {
    object func, res;
    if (Ops.TryGetAttr(self, SymbolTable.OpInPlace%(clrName)s, out func) && Ops.TryCall(func, other, out res)) return res;
    return base.InPlace%(clrName)s(self, other);
}"""

REFLECTED_CMP = """
public override object %(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.Op%(clrName)s, out func)) return Ops.Call(func, other);
    return base.%(clrName)s(self, other);
}"""

REFLECTED_UNARY = """
public override object %(clrName)s(object self) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.Op%(clrName)s, out func)) return Ops.Call(func);
    return Ops.NotImplemented;
}"""

def reflected_ops(cw):
    for op in ops:
        if not isinstance(op, Operator): continue
        if op.symbol in ['==', '!=', "<>"]: continue
        op.genDynamicTypeOp(cw, REFLECTED, REFLECTED_CMP)

    cw.write(REFLECTED_UNARY % ({'clrName' : 'Negate'}))
    cw.write(REFLECTED_UNARY % ({'clrName' : 'OnesComplement'}))


CodeGenerator("ReflectedType Binary Operators", reflected_ops).doit()

def ops_generator_usertype(cw, basicTemplate, cmpTemplate):
    for op in ops:
        if not isinstance(op, Operator): continue
        if op.name == "lg": continue
        if op.name == "eq" or op.name == "ne": continue
        op.genDynamicTypeOp(cw, basicTemplate, cmpTemplate)


USERTYPE = """
public override object %(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.Op%(clrName)s, out func)) {
        object ret;
        if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
    }
    return Ops.NotImplemented;
}
public override object Reverse%(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.OpReverse%(clrName)s, out func)){
        object ret;
        if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
    }
    return Ops.NotImplemented;

}
public override object InPlace%(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.OpInPlace%(clrName)s, out func)) {
        object ret;
        if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
    }
    return base.InPlace%(clrName)s(self, other);
}"""

USERTYPE_CMP = """
public override object %(clrName)s(object self, object other) {
    object func;
    if (Ops.TryGetAttr(self, SymbolTable.Op%(clrName)s, out func)) {
        object ret;
        if (Ops.TryCall(func, other, out ret) && ret != Ops.NotImplemented) return ret;
    }
    return Ops.NotImplemented;
}"""

def usertype_ops(cw):
    return ops_generator_usertype(cw, USERTYPE, USERTYPE_CMP)

CodeGenerator("UserType Binary Operators", usertype_ops).doit()