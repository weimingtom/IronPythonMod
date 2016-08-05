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
from generate import CodeGenerator, CodeWriter

binaries = [('+',   'add',      4,  'Add',          'Add',          '+'),
            ('-',   'sub',      4,  'Subtract',     'Subtract',     '-'),
            ('**',  'pow',      6,  'Power',        'Power',        None),
            ('*',   'mul',      5,  'Multiply',     'Multiply',     '*'),
            ('/',   'div',      5,  'Divide',       'Divide',       '/'),
            ('//',  'floordiv', 5,  'FloorDivide',  'Divide',       '/'),
            ('///', 'truediv',  5,  'TrueDivide',   'Divide',       '/'),
            ('%',   'mod',      5,  'Mod',          'Mod',          '%'),
            ('<<',  'lshift',   3,  'LeftShift',    'LeftShift',    '<<'),
            ('>>',  'rshift',   3,  'RightShift',   'RightShift',   '>>'),
            ('&',   'and',      2,  'BitwiseAnd',   'BitwiseAnd',   '&'),
            ('|',   'or',       0,  'BitwiseOr',    'BitwiseOr',    '|'),
            ('^',   'xor',      1,  'Xor',          'Xor',          '^')]


long_base_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) return x %(sym)s ((int)other);
    if (other is Complex64) return x %(sym)s ((Complex64)other);
    if (other is double) return x %(sym)s ((double)other);
    if (other is BigInteger) return x %(sym)s ((BigInteger)other);
    if (other is bool) return x %(sym)s ((bool) other ? 1 : 0);
    if (other is long) return x %(sym)s ((long)other);
    if (other is ExtensibleInt) return x %(sym)s (((ExtensibleInt)other).value);
    if (other is ExtensibleFloat) return x %(sym)s (((ExtensibleFloat)other).value);
    if (other is ExtensibleComplex) return x %(sym)s ((ExtensibleComplex)other).value;
    if (other is byte) return x %(sym)s (int)((byte)other);
    return Ops.NotImplemented;
}
"""

long_code_altname = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) return %(altname)s(x, (int)other);
    if (other is Complex64) {
        Complex64 y = (Complex64)other;
        if(y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    }
    if (other is double) return FloatOps.%(name)s(x, (double)other);
    if (other is bool) return %(altname)s(x, (bool)other ? 1 : 0);
    if (other is long) return %(altname)s(x, (long)other);
    if (other is BigInteger) return %(altname)s(x, (BigInteger)other);
    if (other is ExtensibleInt) return %(altname)s(x, ((ExtensibleInt)other).value);
    if (other is ExtensibleComplex) {
        Complex64 y = ((ExtensibleComplex)other).value;
        if(y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    }
    if (other is byte) return %(altname)s(x, (int)((byte)other));
    if (other is ExtensibleFloat) return FloatOps.%(name)s(x, ((ExtensibleFloat)other).value);
    return Ops.NotImplemented;
}


[PythonName("__r%(pyName)s__")]
public static object Reverse%(name)s(%(type)s x, object other) {
    if (other is int) return IntOps.%(altname)s((int)other, x);
    if (other is Complex64) {
        Complex64 y = (Complex64)other;
        if(y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(altname)s(y, Complex64.MakeReal(x));
    }
    if (other is double) return FloatOps.%(name)s((double)other, x);
    if (other is bool) return %(altname)s((bool)other ? 1 : 0, x);
    if (other is long) return %(altname)s((long)other, x);
    if (other is BigInteger) return %(altname)s((BigInteger)other, x);
    if (other is ExtensibleInt) return %(altname)s(((ExtensibleInt)other).value, x);
    if (other is ExtensibleComplex) {
        Complex64 y = ((ExtensibleComplex)other).value;
        if(y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(y, Complex64.MakeReal(x));
    }
    if (other is byte) return IntOps.%(altname)s((int)((byte)other), x);
    if (other is ExtensibleFloat) return FloatOps.%(name)s(((ExtensibleFloat)other).value, x);
    return Ops.NotImplemented;
}

"""

long_code_integers = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(BigInteger x, object other) {
    if (other is BigInteger) return x %(sym)s (BigInteger)other;
    if (other is long) return x %(sym)s (long)other;
    if (other is int) return x %(sym)s (int)other;
    if (other is bool) return x %(sym)s ((bool)other ? 1 : 0);
    if (other is ExtensibleInt) return x %(sym)s ((ExtensibleInt)other).value;
    if (other is byte) return x %(sym)s (int)((byte)other);
    return Ops.NotImplemented;
}
"""

long_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(BigInteger x, object other) {
    if (other is int) return %(name)s(x, (int)other);
    if (other is BigInteger) return %(name)s(x, (BigInteger)other);
    if (other is double) return %(name)s(x, (double)other);
    if (other is Complex64) return ComplexOps.%(name)s(x, (Complex64)other);
    if (other is bool) return %(name)s(x, (bool)other ? 1 : 0);
    if (other is long) return %(name)s(x, (long)other);
    if (other is ExtensibleInt) return %(name)s(x, ((ExtensibleInt)other).value);
    if (other is ExtensibleFloat) return %(name)s(x, ((ExtensibleFloat)other).value);
    if (other is ExtensibleComplex) return %(name)s(x, ((ExtensibleComplex)other).value);
    if (other is byte) return %(name)s(x, (int)((byte)other));
    return Ops.NotImplemented;
}
"""

float_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(double x, object other) {
    if (other is double) return x %(sym)s ((double)other);
    if (other is int) return x %(sym)s ((int)other);
    if (other is Complex64) return ComplexOps.%(name)s(Complex64.MakeReal(x), (Complex64)other);
    if (other is BigInteger) return x %(sym)s ((BigInteger)other);
    if (other is float) return x %(sym)s ((float)other);
    if (other is ExtensibleFloat) return x %(sym)s ((ExtensibleFloat)other).value;
    if (other is string) return Ops.NotImplemented;
    if (other is IConvertible) {
        double y = ((IConvertible)other).ToDouble(null);
        return x %(sym)s y;
    }
    if (other is long) return x %(sym)s ((long)other);
    if (other is ExtensibleInt) return x %(sym)s ((ExtensibleInt)other).value;
    if (other is ExtensibleComplex) return ComplexOps.%(name)s(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
    return Ops.NotImplemented;
}
"""

float_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(double x, object other) {
    if (other is double) return %(name)s(x, ((double)other));
    if (other is int) return %(name)s(x, ((int)other));
    if (other is Complex64) return ComplexOps.%(name)s(Complex64.MakeReal(x), (Complex64)other);
    if (other is BigInteger) return %(name)s(x, ((BigInteger)other));
    if (other is bool) return %(name)s(x, (bool)other ? 1.0 : 0.0);
    if (other is float) return %(name)s(x, ((float)other));
    if (other is ExtensibleFloat) return %(name)s(x, ((ExtensibleFloat)other).value);
    if (other is long) return %(name)s(x, ((long)other));
    if (other is ExtensibleComplex) return ComplexOps.%(name)s(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
    if (other is ExtensibleInt) return %(name)s(x, ((ExtensibleInt)other).value);
    if (other is byte) return %(name)s(x, (int)((byte)other));
   return Ops.NotImplemented;
}
"""


complex_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) {
        return x %(sym)s (int)other;
    } else if (other is Complex64) {
        return x %(sym)s (Complex64)other;
    } else if (other is double) {
        return x %(sym)s (double)other;
    } else if (other is BigInteger) {
        return x %(sym)s (BigInteger)other;
    } else if (other is long) {
        return x %(sym)s (long)other;
    } else if (other is ExtensibleComplex) {
        return x %(sym)s ((ExtensibleComplex)other).value;
    } else if (other is ExtensibleInt) {
        return x %(sym)s ((ExtensibleInt)other).value;
    } else if (other is ExtensibleFloat) {
        return x %(sym)s ((ExtensibleFloat)other).value;
    } else if(other is string) {
        return Ops.NotImplemented;
    } else if (other is IConvertible) {
        double y = ((IConvertible)other).ToDouble(null);
        return x %(sym)s y;
    }
    return Ops.NotImplemented;
}
"""

complex_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) return %(name)s(x, (Complex64) ((int)other));
    if (other is Complex64) return %(name)s(x, (Complex64) other);
    if (other is double) return %(name)s(x, (Complex64) ((double) other));
    if (other is BigInteger) return %(name)s(x, (Complex64) ((BigInteger) other));
    if (other is bool) return %(name)s(x, (Complex64)((bool)other ? 1 : 0));
    if (other is long) return %(name)s(x, (Complex64)((long) other));
    if (other is ExtensibleComplex) return %(name)s(x, ((ExtensibleComplex)other).value);
    if (other is ExtensibleFloat) return %(name)s(x, (Complex64)((ExtensibleFloat)other).value);
    if (other is ExtensibleInt) return %(name)s(x, (Complex64)((ExtensibleInt)other).value);
    if (other is byte) return %(name)s(x, (Complex64) (int)((byte)other));
    return Ops.NotImplemented;
}
"""

int_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is BigInteger) {
        return BigInteger.Create(x) %(sym)s (BigInteger)other;
    } else if (other is double) {
        return x %(sym)s (double)other;
    } else if (other is Complex64) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), other);
    } else if (other is bool) {
        bool b = (bool)other;
        return x %(sym)s (b ? 1 : 0);
    } else if (other is long) {
        long y = (long)other;
        try {
            return checked(x %(sym)s y);
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is float) {
        return x %(sym)s (float)other;
    } else if (other is byte) {
        return x %(sym)s (byte)other;
    } else if (other is ExtensibleInt) {
        int y = ((ExtensibleInt)other).value;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is ExtensibleFloat) {
        return x %(sym)s ((ExtensibleFloat)other).value;
    } else if (other is ExtensibleComplex) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), (ExtensibleComplex)other);
    } else if (other is byte) {
        int y = (byte)other;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    }
    return Ops.NotImplemented;
}
"""

int_code_divide = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return LongOps.%(name)s(BigInteger.Create(x) , y);
        }
    } else if (other is BigInteger) {
        return LongOps.%(name)s(BigInteger.Create(x), (BigInteger)other);
    } else if (other is double) {
        return FloatOps.%(name)s(x, (double)other);
    } else if (other is Complex64) {
        Complex64 y = (Complex64)other;
        if(y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    } else if (other is bool) {
        bool b = (bool)other;
        return x %(altsym)s (b ? 1 : 0);
    } else if (other is long) {
        long y = (long)other;
        try {
            return %(altname)s(x, y);
        } catch (OverflowException) {
            return LongOps.%(name)s(BigInteger.Create(x), y);
        }
    } else if (other is float) {
        return FloatOps.%(name)s(x, (float)other);
    } else if (other is byte) {
        return Ops.%(titleType)s2Object(%(altname)s(x, (int)((byte)other)));
    } else if (other is ExtensibleInt) {
        int y = ((ExtensibleInt)other).value;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return LongOps.%(name)s(BigInteger.Create(x) , y);
        }
    } else if (other is ExtensibleFloat) {
        return FloatOps.%(name)s(x, ((ExtensibleFloat)other).value);
    } else if (other is ExtensibleComplex) {
        Complex64 y = ((ExtensibleComplex)other).value;
        if(y.IsZero) throw Ops.ZeroDivisionError();
        return ComplexOps.%(name)s(Complex64.MakeReal(x), y);
    }
    return Ops.NotImplemented;
}
"""

int_code_bitwise = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) {
        return Ops.%(titleType)s2Object(x %(sym)s (int)other);
    } else if (other is long) {
        long lx = (long)x;
        return lx %(sym)s (long)other;
    } else if (other is BigInteger) {
        return BigInteger.Create(x) %(sym)s (BigInteger)other;
    } else if (other is bool) {
        return Ops.%(titleType)s2Object(x %(sym)s ((bool)other ? 1 : 0));
    } else if (other is ExtensibleInt) {
        return Ops.%(titleType)s2Object(x %(sym)s ((ExtensibleInt)other).value);
    } else if (other is byte) {
        return Ops.%(titleType)s2Object(x %(sym)s (int)((byte)other));
    }
    return Ops.NotImplemented;
}
"""

int64_code = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is BigInteger) {
        return BigInteger.Create(x) %(sym)s (BigInteger)other;
    } else if (other is double) {
        return x %(sym)s (double)other;
    } else if (other is Complex64) {
        return Complex64.MakeReal(x) %(sym)s (Complex64)other;
    } else if (other is bool) {
        int y = (bool)other ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is long) {
        long y = (long)other;
        try {
            return checked(x %(sym)s y);
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is float) {
        return x %(sym)s (float)other;
    } else if (other is ExtensibleInt) {
        int y = ((ExtensibleInt)other).value;
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    } else if (other is ExtensibleFloat) {
        return x %(sym)s ((ExtensibleFloat)other).value;
    } else if (other is ExtensibleComplex) {
        return Complex64.MakeReal(x) %(sym)s ((ExtensibleComplex)other).value;
    } else if (other is byte) {
        int y = (int)((byte)other);
        try {
            return Ops.%(titleType)s2Object(checked(x %(sym)s y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(sym)s y;
        }
    }
    return Ops.NotImplemented;
}
"""

int64_code_altname = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) {
        int y = (int)other;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    } else if (other is BigInteger) {
        return LongOps.%(name)s(BigInteger.Create(x), (BigInteger)other);
    } else if (other is double) {
        return FloatOps.%(name)s(x, (double)other);
    } else if (other is Complex64) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), (Complex64)other);
    } else if (other is bool) {
        int y = (bool)other ? 1 : 0;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }    
    } else if (other is long) {
        long y = (long)other;
        try {
            return %(altname)s(x, y);
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    } else if (other is float) {
        return FloatOps.%(name)s(x, (float)other);
    } else if (other is ExtensibleInt) {
        int y = ((ExtensibleInt)other).value;
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    } else if (other is ExtensibleFloat) {
        return FloatOps.%(name)s(x, ((ExtensibleFloat)other).value);
    } else if (other is ExtensibleComplex) {
        return ComplexOps.%(name)s(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
    } else if (other is byte) {
        int y = (int)((byte)other);
        try {
            return Ops.%(titleType)s2Object(%(altname)s(x, y));
        } catch (OverflowException) {
            return BigInteger.Create(x) %(altsym)s y;
        }
    }

    return Ops.NotImplemented;
}
"""

int64_code_bitwise = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(%(type)s x, object other) {
    if (other is int) {
        long y = (long)(int)other;
        return Ops.%(titleType)s2Object(x %(sym)s y);
    } else if (other is long) {
        return x %(sym)s (long)other;
    } else if (other is BigInteger) {
        return BigInteger.Create(x) %(sym)s (BigInteger)other;
    } else if (other is bool) {
        return Ops.%(titleType)s2Object(x %(sym)s ((bool)other ? 1L : 0L));
    } else if (other is ExtensibleInt) {
        long y = (long)((ExtensibleInt)other).value;
        return Ops.%(titleType)s2Object(x %(sym)s y);
    } else if (other is byte) {
        return Ops.%(titleType)s2Object(x %(sym)s (long)((byte)other));
    }
    return Ops.NotImplemented;
}
"""

int64_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(long x, object other) {
    if (other is int) return %(name)s(x, (int)other);
    if (other is BigInteger) return %(name)s(x, (BigInteger)other);
    if (other is long) return %(name)s(x, (long)other);
    if (other is double) return %(name)s(x, (double)other);
    if (other is Complex64) return ComplexOps.%(name)s(x, (Complex64)other);
    if (other is bool) return %(name)s(x, (bool)other ? 1 : 0); 
    if (other is float) return %(name)s(x, (float)other);
    if (other is ExtensibleInt) return %(name)s(x, ((ExtensibleInt)other).value);
    if (other is ExtensibleFloat) return %(name)s(x, ((ExtensibleFloat)other).value);
    if (other is ExtensibleComplex) return %(name)s(x, ((ExtensibleComplex)other).value);
    if (other is byte) return %(name)s(x, (int)((byte)other));
    return Ops.NotImplemented;
}
"""

int_code_m = """
[PythonName("__%(pyName)s__")]
public static object %(name)s(int x, object other) {
    if (other is int) {
        return %(name)s(x, (int)other);
    } else if (other is double) {
        return %(name)s(x, (double)other);
    } else if (other is long) {
        return %(name)s(x, (long)other);
    } else if (other is BigInteger) {
        return %(name)s(x, (BigInteger)other);
    } else if (other is bool) {
        return %(name)s(x, (bool)other ? 1 : 0);
    } else if (other is Complex64) {
        return %(name)s(x, (Complex64)other);
    } else if (other is ExtensibleInt) {
        return %(name)s(x, ((ExtensibleInt)other).value);
    } else if (other is ExtensibleFloat) {
        return %(name)s(x, ((ExtensibleFloat)other).value);
    } else if (other is ExtensibleComplex) {
        return %(name)s(x, ((ExtensibleComplex)other).value);
    }
    return Ops.NotImplemented;
}
"""

float_custom_syms = ('**', '/', '%', '//')
float_im_syms = ('///', )

int_custom_syms = ('>>','<<')
int32_custom_syms = ('**',)
long_use_altname = ('/', '%', '//')
int_bitwise = ('&', '|', '^')

int_divide = ('/','%', '//')

im_syms = ('**', '///', )
int32_im_syms = ('///', )

i_syms = ('<<', '>>')
any_integer_syms = ('&', '|', '^')
integer_syms =  any_integer_syms + i_syms

complex_custom_syms = ('//', '%')

class GenFuncs:
    def __init__(self, tname, sym_map, default_template, default_div, swap_syms = {}):
        self.tname = tname
        self.default_div = default_div
        self.swap_syms = swap_syms
        self.sym_map = {}
        for sym, name, prec, cname, altname, altsym in binaries:
            self.sym_map[sym] = default_template

        for sym_list, template in sym_map.items():
            for sym in sym_list:
                self.sym_map[sym] = template
        if tname == 'int': self.nextType = 'long'
        else: self.nextType = 'BigInteger'

    def __call__(self, cw):
        for sym, name, prec, cname, altname, altsym in binaries:
            sym = self.swap_syms.get(sym, sym)
            template = self.sym_map[sym]
            if template is not None:
                cw.write(template,
                         name=cname,
                         sym=sym,
                         type=self.tname,
                         titleType = self.tname.title(),
                         nextType=self.nextType,
                         pyName=name,
                         altname=altname,
                         altsym=altsym
                         )


CodeGenerator("LongOps",
    GenFuncs('BigInteger', {
        long_use_altname:long_code_altname,
        int_custom_syms:None,
        im_syms:long_code_m,
        any_integer_syms:long_code_integers
    }, long_base_code, 'FloorDivide')).doit()


CodeGenerator("IntOps",
    GenFuncs('int', {
        int_divide:int_code_divide,
        int_bitwise:int_code_bitwise,
        int32_custom_syms+int_custom_syms:None,
        int32_im_syms:int_code_m
    }, int_code, 'FloorDivide')).doit()

CodeGenerator("Int64Ops",
    GenFuncs('long', {
        long_use_altname:int64_code_altname,
        int_bitwise:int64_code_bitwise,
        int_custom_syms:None,
        im_syms:int64_code_m
    }, int64_code, 'FloorDivide')).doit()


CodeGenerator("FloatOps",
    GenFuncs('double', {
        integer_syms+float_custom_syms:None,
        float_im_syms:float_code_m
    }, float_code, "TrueDivide")).doit()

CodeGenerator("ComplexOps",
    GenFuncs('Complex64', {
        integer_syms:None,
        im_syms:complex_code_m,
        ('//', '/', '%'): None
    }, complex_code, 'TrueDivide')).doit()
