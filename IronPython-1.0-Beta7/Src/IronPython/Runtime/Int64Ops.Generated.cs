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

using IronMath;

namespace IronPython.Runtime {
    public static partial class Int64Ops {
        #region Generated Int64Ops

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(long x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is BigInteger) {
                return BigInteger.Create(x) + (BigInteger)other;
            } else if (other is double) {
                return x + (double)other;
            } else if (other is Complex64) {
                return Complex64.MakeReal(x) + (Complex64)other;
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x + y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is float) {
                return x + (float)other;
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is ExtensibleFloat) {
                return x + ((ExtensibleFloat)other).value;
            } else if (other is ExtensibleComplex) {
                return Complex64.MakeReal(x) + ((ExtensibleComplex)other).value;
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(long x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is BigInteger) {
                return BigInteger.Create(x) - (BigInteger)other;
            } else if (other is double) {
                return x - (double)other;
            } else if (other is Complex64) {
                return Complex64.MakeReal(x) - (Complex64)other;
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x - y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is float) {
                return x - (float)other;
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is ExtensibleFloat) {
                return x - ((ExtensibleFloat)other).value;
            } else if (other is ExtensibleComplex) {
                return Complex64.MakeReal(x) - ((ExtensibleComplex)other).value;
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__pow__")]
        public static object Power(long x, object other) {
            if (other is int) return Power(x, (int)other);
            if (other is BigInteger) return Power(x, (BigInteger)other);
            if (other is long) return Power(x, (long)other);
            if (other is double) return Power(x, (double)other);
            if (other is Complex64) return ComplexOps.Power(x, (Complex64)other);
            if (other is bool) return Power(x, (bool)other ? 1 : 0); 
            if (other is float) return Power(x, (float)other);
            if (other is ExtensibleInt) return Power(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleFloat) return Power(x, ((ExtensibleFloat)other).value);
            if (other is ExtensibleComplex) return Power(x, ((ExtensibleComplex)other).value);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(long x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is BigInteger) {
                return BigInteger.Create(x) * (BigInteger)other;
            } else if (other is double) {
                return x * (double)other;
            } else if (other is Complex64) {
                return Complex64.MakeReal(x) * (Complex64)other;
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x * y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is float) {
                return x * (float)other;
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is ExtensibleFloat) {
                return x * ((ExtensibleFloat)other).value;
            } else if (other is ExtensibleComplex) {
                return Complex64.MakeReal(x) * ((ExtensibleComplex)other).value;
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__div__")]
        public static object Divide(long x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is BigInteger) {
                return LongOps.Divide(BigInteger.Create(x), (BigInteger)other);
            } else if (other is double) {
                return FloatOps.Divide(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.Divide(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is float) {
                return FloatOps.Divide(x, (float)other);
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is ExtensibleFloat) {
                return FloatOps.Divide(x, ((ExtensibleFloat)other).value);
            } else if (other is ExtensibleComplex) {
                return ComplexOps.Divide(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__floordiv__")]
        public static object FloorDivide(long x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is BigInteger) {
                return LongOps.FloorDivide(BigInteger.Create(x), (BigInteger)other);
            } else if (other is double) {
                return FloatOps.FloorDivide(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is float) {
                return FloatOps.FloorDivide(x, (float)other);
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            } else if (other is ExtensibleFloat) {
                return FloatOps.FloorDivide(x, ((ExtensibleFloat)other).value);
            } else if (other is ExtensibleComplex) {
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(Divide(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) / y;
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(long x, object other) {
            if (other is int) return TrueDivide(x, (int)other);
            if (other is BigInteger) return TrueDivide(x, (BigInteger)other);
            if (other is long) return TrueDivide(x, (long)other);
            if (other is double) return TrueDivide(x, (double)other);
            if (other is Complex64) return ComplexOps.TrueDivide(x, (Complex64)other);
            if (other is bool) return TrueDivide(x, (bool)other ? 1 : 0); 
            if (other is float) return TrueDivide(x, (float)other);
            if (other is ExtensibleInt) return TrueDivide(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleFloat) return TrueDivide(x, ((ExtensibleFloat)other).value);
            if (other is ExtensibleComplex) return TrueDivide(x, ((ExtensibleComplex)other).value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(long x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            } else if (other is BigInteger) {
                return LongOps.Mod(BigInteger.Create(x), (BigInteger)other);
            } else if (other is double) {
                return FloatOps.Mod(x, (double)other);
            } else if (other is Complex64) {
                return ComplexOps.Mod(Complex64.MakeReal(x), (Complex64)other);
            } else if (other is bool) {
                int y = (bool)other ? 1 : 0;
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }    
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Mod(x, y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            } else if (other is float) {
                return FloatOps.Mod(x, (float)other);
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            } else if (other is ExtensibleFloat) {
                return FloatOps.Mod(x, ((ExtensibleFloat)other).value);
            } else if (other is ExtensibleComplex) {
                return ComplexOps.Mod(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
            } else if (other is byte) {
                int y = (int)((byte)other);
                try {
                    return Ops.Long2Object(Mod(x, y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) % y;
                }
            }

            return Ops.NotImplemented;
        }


        [PythonName("__and__")]
        public static object BitwiseAnd(long x, object other) {
            if (other is int) {
                long y = (long)(int)other;
                return Ops.Long2Object(x & y);
            } else if (other is long) {
                return x & (long)other;
            } else if (other is BigInteger) {
                return BigInteger.Create(x) & (BigInteger)other;
            } else if (other is bool) {
                return Ops.Long2Object(x & ((bool)other ? 1L : 0L));
            } else if (other is ExtensibleInt) {
                long y = (long)((ExtensibleInt)other).value;
                return Ops.Long2Object(x & y);
            } else if (other is byte) {
                return Ops.Long2Object(x & (long)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__or__")]
        public static object BitwiseOr(long x, object other) {
            if (other is int) {
                long y = (long)(int)other;
                return Ops.Long2Object(x | y);
            } else if (other is long) {
                return x | (long)other;
            } else if (other is BigInteger) {
                return BigInteger.Create(x) | (BigInteger)other;
            } else if (other is bool) {
                return Ops.Long2Object(x | ((bool)other ? 1L : 0L));
            } else if (other is ExtensibleInt) {
                long y = (long)((ExtensibleInt)other).value;
                return Ops.Long2Object(x | y);
            } else if (other is byte) {
                return Ops.Long2Object(x | (long)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__xor__")]
        public static object Xor(long x, object other) {
            if (other is int) {
                long y = (long)(int)other;
                return Ops.Long2Object(x ^ y);
            } else if (other is long) {
                return x ^ (long)other;
            } else if (other is BigInteger) {
                return BigInteger.Create(x) ^ (BigInteger)other;
            } else if (other is bool) {
                return Ops.Long2Object(x ^ ((bool)other ? 1L : 0L));
            } else if (other is ExtensibleInt) {
                long y = (long)((ExtensibleInt)other).value;
                return Ops.Long2Object(x ^ y);
            } else if (other is byte) {
                return Ops.Long2Object(x ^ (long)((byte)other));
            }
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
