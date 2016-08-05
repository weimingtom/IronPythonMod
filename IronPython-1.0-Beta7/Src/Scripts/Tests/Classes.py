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

from Util.Debug import *

class C: pass

setdict = C.__dict__
C.__dict__ = setdict

o1 = C()

class C:
    def m(self):
        return 42

o2 = C()
Assert(42 == o2.m())

Assert(o2.__class__ is C)
Assert(o2.__class__ is not o1.__class__)

if is_cli:
    import System
    class MyList(System.Collections.ArrayList):
        def get0(self):
            return self[0]

    l = MyList()
    index = l.Add(22)
    Assert(l.get0() == 22)

class C:pass

C.v = 10

Assert(C.v == 10)

success = 0
try:
    x = C.x
except AttributeError:
    success = 1
Assert(success == 1)


class A:
    def __init__(self, height=20, width=30):
        self.area = height * width

a = A()
Assert(a.area == 600)
a = A(2,3)
Assert(a.area == 6)
a = A(2)
Assert(a.area == 60)
a = A(width = 2)
Assert(a.area == 40)

class C:
    def __init__(self, name, flag):
        self.f = file(name, flag)
    def __getattr__(self, name):
        return getattr(self.f, name)

c=C("x.txt", "w")
c.write("Hello\n")
c.close()
c=C("x.txt", "r")
Assert(c.readline() == "Hello\n")
c.close()

def count_elem(d,n):
    count = 0
    for e in d:
        if e == n:
            count += 1
    return count

# Dictionary and new style classes

class class_n(object):
    val1 = "Value"
    def __init__(self):
        self.val2 = self.val1

inst_n = class_n()
Assert(inst_n.val2 == "Value")
Assert(not 'val2' in dir(class_n))
Assert('val1' in dir(class_n))
Assert('val2' in dir(inst_n))
Assert('val1' in dir(inst_n))
Assert('val2' in inst_n.__dict__)
Assert(inst_n.__dict__['val2'] == "Value")
Assert(count_elem(dir(inst_n), "val1") == 1)
inst_n.val1 = 20
Assert(count_elem(dir(inst_n), "val1") == 1)

# old style classes:

class class_o:
    val1 = "Value"
    def __init__(self):
        self.val2 = self.val1

inst_o = class_o()
Assert('val1' in dir(class_o))
Assert(not 'val2' in dir(class_o))
Assert('val1' in dir(inst_o))
Assert('val2' in dir(inst_o))
Assert('val2' in inst_o.__dict__)
Assert(inst_o.__dict__['val2'] == "Value")
Assert(count_elem(dir(inst_o), "val1") == 1)
inst_n.val1 = 20
Assert(count_elem(dir(inst_o), "val1") == 1)


class C:
    def x(self):
        return 'C.x'
    def y(self):
        return 'C.y'

class D:
    def z(self):
        return 'D.z'

c = C()
AreEqual(c.x(), "C.x")
AreEqual(c.y(), "C.y")

# verify repr and str on old-style class objects have the right format:

# bug# 795
AreEqual(str(C), __name__+'.C')
AreEqual(repr(C).index('<class '+__name__+'.C at 0x'), 0)

success=0
try:
    c.z()
except AttributeError:
    success=1
Assert(success==1)

C.__bases__+=(D,)

AreEqual(c.z(), "D.z")

try:
    import nt
    nt.unlink("x.txt")
except:
    pass

# both of these shouldn't throw

class DirInInit(object):
    def __init__(self):
        dir(self)

a = DirInInit()


class _PrivClass(object):
    def __Mangled(self):
            pass
    def __init__(self):
            a = self.__Mangled

a = _PrivClass()

# Inheritance/Attrs/Dir

class foo:
    def foofunc(self):
        return "foofunc"

class bar(foo):
    def barfunc(self):
        return "barfunc"

class baz(foo, bar):
    def bazfunc(self):
        return "bazfunc"

Assert('foofunc' in dir(foo))
Assert(dir(foo).count('__doc__') == 1)
Assert(dir(foo).count('__module__') == 1)
Assert(len(dir(foo)) == 3)
Assert('foofunc' in dir(bar))
Assert('barfunc' in dir(bar))
Assert(dir(bar).count('__doc__') == 1)
Assert(dir(bar).count('__module__') == 1)
Assert(len(dir(bar)) == 4)
Assert('foofunc' in dir(baz))
Assert('barfunc' in dir(baz))
Assert('bazfunc' in dir(baz))
Assert(dir(baz).count('__doc__') == 1)
Assert(dir(baz).count('__module__') == 1)
Assert(len(dir(baz)) == 5)

bz = baz()
Assert('foofunc' in dir(bz))
Assert('barfunc' in dir(bz))
Assert('bazfunc' in dir(bz))
Assert(dir(bz).count('__doc__') == 1)
Assert(dir(bz).count('__module__') == 1)
Assert(len(dir(bz)) == 5)

bz.__module__ = "MODULE"
Assert(bz.__module__ == "MODULE")
bz.__module__ = "SOMEOTHERMODULE"
Assert(bz.__module__ == "SOMEOTHERMODULE")
bz.__module__ = 33
Assert(bz.__module__ == 33)
bz.__module__ = [2, 3, 4]
Assert(bz.__module__ == [2, 3 , 4])


# verify we don't access __getattr__ while creating an old
# style class.

class C:
    def __getattr__(self,name):
        return globals()[name]

a = C()

############################################################
if is_cli:
    #
    # Verify we can inherit from a class that inherits from an interface
    #

    class MyComparer(System.Collections.IComparer):
        def Compare(self, x, y): return 0
    class MyDerivedComparer(MyComparer): pass
    class MyFurtherDerivedComparer(MyDerivedComparer): pass

    # Check that MyDerivedComparer and MyFurtherDerivedComparer can be used as an IComparer
    s = System.Collections.SortedList(MyComparer())
    s = System.Collections.SortedList(MyDerivedComparer())
    s = System.Collections.SortedList(MyFurtherDerivedComparer())
    
    #
    # Verify that changing __bases__ works
    #

    class MyExceptionComparer(System.Exception, System.Collections.IComparer):
        def Compare(self, x, y): return 0
    e = MyExceptionComparer()
    
    class OldType:
        def OldTypeMethod(self): return "OldTypeMethod"
    class NewType:
        def NewTypeMethod(self): return "NewTypeMethod"
    class MyOtherExceptionComparer(System.Exception, System.Collections.IComparer, OldType, NewType):
        def Compare(self, x, y): return 0
    MyExceptionComparer.__bases__ = MyOtherExceptionComparer.__bases__
    AreEqual(e.OldTypeMethod(), "OldTypeMethod")
    AreEqual(e.NewTypeMethod(), "NewTypeMethod")
    Assert(isinstance(e, System.Exception))
    Assert(isinstance(e, System.Collections.IComparer))
    Assert(isinstance(e, MyExceptionComparer))
    
    class MyIncompatibleExceptionComparer(System.Exception, System.Collections.IComparer, System.IDisposable):
        def Compare(self, x, y): return 0
        def Displose(self): pass
    try:
        MyExceptionComparer.__bases__ = MyIncompatibleExceptionComparer.__bases__
        Assert(None)
    except TypeError:
        (exc_type, exc_value, exc_traceback) = sys.exc_info()
        AreEqual(exc_value.msg, "cannot add CLI type <type 'IDisposable'> to <class '%s.MyExceptionComparer'>.__bases__" % __name__)
    
    # Inherting from an open generic instantiation should fail with a good error message
    try:
        class Foo(System.Collections.Generic.IEnumerable): pass
    except TypeError:
        (exc_type, exc_value, exc_traceback) = sys.exc_info()
        Assert(exc_value.msg.__contains__("cannot inhert from open generic instantiation"))

# use exec to define methods on classes:

class oldclasswithexec:
    exec "def oldexecmethod(self): return 'result of oldexecmethod'"

Assert('oldexecmethod' in dir(oldclasswithexec))
AreEqual(oldclasswithexec().oldexecmethod(), 'result of oldexecmethod')

class newclasswithexec(object):
    exec "def newexecmethod(self): return 'result of newexecmethod'"

Assert('newexecmethod' in dir(newclasswithexec))
AreEqual(newclasswithexec().newexecmethod(), 'result of newexecmethod')


def func1():
	__name__ = "wrong"
	class C: pass
	return C()

def func2():
	class C: pass
	return C()

def func3():
	global __name__ 
	__name__ = "right"
	class C: pass
	return C()
	
	
AreEqual(func1().__module__, func2().__module__)

__name__ = "fake"
AreEqual(func1().__module__, "fake")

AreEqual(func3().__module__, "right")

###################################################################################
# tests to verify that Symbol dictionaries do the right thing in dynamic scenarios

def CheckDictionary(C): 
    # add a new attribute to the type...
    C.newClassAttr = 'xyz'
    AreEqual(C.newClassAttr, 'xyz')
    
    # add non-string index into the class and instance dictionary
    a = C()
    a.__dict__[1] = '1'
    C.__dict__[2] = '2'
    AreEqual(a.__dict__.has_key(1), True)
    AreEqual(C.__dict__.has_key(2), True)
    AreEqual(dir(a).__contains__(1), True)
    AreEqual(dir(a).__contains__(2), True)
    AreEqual(dir(C).__contains__(2), True)
    AreEqual(repr(a.__dict__), "{1: '1'}")
    AreEqual(repr(C.__dict__).__contains__("2: '2'"), True)
    
    # replace a class dictionary (containing non-string keys) w/ a normal dictionary
    C.newTypeAttr = 1
    AreEqual(hasattr(C, 'newTypeAttr'), True)
    C.__dict__ = dict(C.__dict__)  
    AreEqual(hasattr(C, 'newTypeAttr'), True)
    
    # replace an instance dictionary (containing non-string keys) w/ a new one.
    a.newInstanceAttr = 1
    AreEqual(hasattr(a, 'newInstanceAttr'), True)
    a.__dict__  = dict(a.__dict__)
    AreEqual(hasattr(a, 'newInstanceAttr'), True)

    a.abc = 'xyz'  
    AreEqual(hasattr(a, 'abc'), True)
    AreEqual(getattr(a, 'abc'), 'xyz')
    

class OldClass: 
    def __init__(self):  pass

class NewClass(object): 
    def __init__(self):  pass

CheckDictionary(OldClass)
CheckDictionary(NewClass)

########################################################
### metaclass tests



# verify we can use a function as a metaclass in the dictionary
recvArgs = None
def funcMeta(*args):
    global recvArgs
    recvArgs = args
    
class foo:
    __metaclass__ = funcMeta

AreEqual(recvArgs, ('foo', (), {'__module__' : __name__, '__metaclass__' : funcMeta}))

class foo(object):
    __metaclass__ = funcMeta

AreEqual(recvArgs, ('foo', (object, ), {'__module__' : __name__, '__metaclass__' : funcMeta}))
        

# verify setting __metaclass__ to default old-style type works

class classType: pass
classType = type(classType)     # get classObj for tests
__metaclass__ = classType
class c: pass
AreEqual(type(c), classType)
del(__metaclass__)


# verify setting __metaclass__ to default new-style type works
__metaclass__ = type
class c: pass
AreEqual(type(c), type)
del(__metaclass__)

# try setting it a different way - by getting it from a type
class c(object): pass
__metaclass__  = type(c)
class xyz: pass
AreEqual(type(xyz), type(c))
del(__metaclass__)

# verify setting __metaclass__ at module scope to a function works
__metaclass__ = funcMeta
recvArgs = None
class foo: pass
AreEqual(recvArgs, ('foo', (), {'__module__' : __name__}))  # note no __metaclass__ becauses its not in our dict

# clean up __metaclass__ for other tests
del(__metaclass__)

##################################################################
# inheritance from both old & new style classes...
class foo: pass

class bar(object): pass

class baz1(foo, bar): pass

class baz2(bar, foo): pass

AreEqual(baz1.__bases__, (foo, bar))
AreEqual(baz2.__bases__, (bar, foo))


# verify calling unbound method w/ new-style class on subclass which 
# new-style also inherits from works.
class foo:
    def func(self): return self

class bar(object, foo):
    def barfunc(self):
            return foo.func(self)

a = bar()
AreEqual(a.barfunc(), a)

############################################################
# mro (method resolution order) support

class A(object): pass

AreEqual(A.__mro__, (A, object))

class B(object): pass

AreEqual(B.__mro__, (B, object))

class C(B): pass

AreEqual(C.__mro__, (C, B, object))

class N(C,B,A): pass

AreEqual(N.__mro__, (N, C, B, A, object))

try:
    class N(A, B,C): pass
    AreEqual(True, False) #failure, shouldn't be possible
except TypeError:
    pass

try:
    class N(A, A): pass
    AreEqual(True, False) # can't derive from same base type
except TypeError:
    pass

######
# verify replacing base classes also updates MRO
class C(object): 
    def __getattribute__(self, name): 
        if(name == 'xyz'): return 'C'
        return super(C, self).__getattribute__(name)

class C1(C): 
    def __getattribute__(self, name): 
        if(name == 'xyz'):  return 'C1'
        return super(C1, self).__getattribute__(name)

class A(object): pass

class B(object):
    def __getattribute__(self, name):
        if(name == 'xyz'): return 'B'
        return super(B, self).__getattribute__(name)

a = C1()
AreEqual(a.xyz, 'C1')

C1.__bases__ = (A,B)
AreEqual(a.xyz, 'C1')

del(C1.__getattribute__)
AreEqual(a.xyz, 'B')

# Testing the class attributes backed by globals

x = 10

class C:
    x = x
    del x
    x = x
    
AreEqual(C.x, 10)
AreEqual(x, 10)

try:
    class C:
        x = x
        del x
        del x
except NameError:
    pass
else:
    Assert("Expecting name error")

AreEqual(x, 10)

class C:
    x = 10
    del x
    b = x
    AreEqual(x, 10)

AreEqual(C.b, 10)
AreEqual(x, 10)


