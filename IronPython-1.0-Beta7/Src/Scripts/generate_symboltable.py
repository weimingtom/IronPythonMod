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

startValue = 46 # ops come first, we see generate_ops to figure out where we start...

fieldList = [
    ('__neg__', 'OpNegate'),
    ('__invert__', 'OpOnesComplement'),

    ('__dict__', 'Dict'),
    ('__module__', 'Module'),
    ('__getattribute__', 'GetAttribute'),
    ('__bases__', 'Bases'),
    ('__subclasses__', 'Subclasses'),
    ('__name__', 'Name'),
    ('__class__', 'Class'),

    ('__builtins__', 'Builtins'),
    
    ('__getattr__', 'GetAttr'),
    ('__setattr__', 'SetAttr'),
    ('__delattr__', 'DelAttr'),
    
    ('__getitem__', 'GetItem'),
    ('__setitem__', 'SetItem'),
    ('__delitem__', 'DelItem'),
    
    ('__init__', 'Init'),
    ('__new__', 'NewInst'),    
    ('__del__', 'Unassign'),
    
    ('__str__', 'String'),
    ('__repr__', 'Repr'),
    
    ('__contains__', 'Contains'),
    ('__len__', 'Length'),
    ('__reversed__', 'Reversed'),
    ('__iter__', 'Iterator'),
    ('__next__', 'Next'),    

    ('__weakref__', 'WeakRef'),
    ('__file__', 'File'),
    ('__import__', 'Import'),
    ('__doc__', 'Doc'),
    ('__call__', 'Call'),
    
    ('__abs__', 'AbsoluteValue'),
    ('__coerce__', 'Coerce'),
    ('__int__', 'ConvertToInt'),
    ('__float__', 'ConvertToFloat'),
    ('__long__', 'ConvertToLong'),
    ('__complex__', 'ConvertToComplex'),
    ('__hex__', 'ConvertToHex'),
    ('__oct__', 'ConvertToOctal'),

    ('__nonzero__', 'NonZero'),
    ('__pos__', 'Positive'),
    
    ('__hash__', 'Hash'),
    ('__cmp__', 'Cmp'),
    
    ('__path__', 'Path'),
    
    ('__get__', 'GetDescriptor'),
    ('__set__', 'SetDescriptor'),
    ('__delete__', 'DeleteDescriptor'),
    ('__all__', 'All'),
    

    ('clsException', 'ClrExceptionKey'),
    ('keys', 'Keys'),
    ('args', 'Arguments'),
    ('write', 'ConsoleWrite'),
    ('readline', 'ConsoleReadLine'),
    ('msg', 'ExceptionMessage'),
    ('filename', 'ExceptionFilename'),
    ('lineno', 'ExceptionLineNumber'),
    ('offset', 'ExceptionOffset'),
    ('text', 'Text'),
    ('softspace', 'Softspace'),
    ('next', 'GeneratorNext'),
    ('setdefaultencoding', 'SetDefaultEncoding'),
    ('exitfunc', 'SysExitFunc'),
    
    ('__metaclass__', 'MetaClass'),
    ('__mro__', 'MethodResolutionOrder'),
    ('__getslice__', 'GetSlice'),
    ('__setslice__', 'SetSlice'),
    ('__delslice__', 'DeleteSlice'),
    ('LastWellKnownId', 'LastWellKnownId')
    ]

def generate_values(cw):
    i = startValue
    for x in fieldList:
        cw.writeline("public const int %sId = %d;" % (x[1],i))
        i = i + 1

def generate_symbols(cw):
    i = startValue
    for x in fieldList:
        cw.writeline("public static readonly SymbolId %s = new SymbolId(%sId);" % (x[1],x[1]))
        i = i + 1

def generate_added(cw):
    i = startValue
    for x in fieldList:
        cw.writeline("StringToId(\"%s\");  // %d" % (x[0], i))
        i = i+1
 
 
CodeGenerator("SymbolTable Other Values", generate_values).doit()
CodeGenerator("SymbolTable Other Added", generate_added).doit()
CodeGenerator("SymbolTable Other Symbols", generate_symbols).doit()
