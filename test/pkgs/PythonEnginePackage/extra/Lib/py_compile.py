"""Routine to "compile" a .py file to a .pyc (or .pyo) file.

This module has intimate knowledge of the format of .pyc files.
"""

import __builtin__
import imp
import marshal
import os
import sys
import traceback

MAGIC = imp.get_magic()

__all__ = ["compile", "main", "PyCompileError"]


class PyCompileError(Exception):
    """Exception raised when an error occurs while attempting to
    compile the file.

    To raise this exception, use

        raise PyCompileError(exc_type,exc_value,file[,msg])

    where

        exc_type:   exception type to be used in error message
                    type name can be accesses as class variable
                    'exc_type_name'

        exc_value:  exception value to be used in error message
                    can be accesses as class variable 'exc_value'

        file:       name of file being compiled to be used in error message
                    can be accesses as class variable 'file'

        msg:        string message to be written as error message
                    If no value is given, a default exception message will be given,
                    consistent with 'standard' py_compile output.
                    message (or default) can be accesses as class variable 'msg'

    """

    def __init__(self, exc_type, exc_value, file, msg=''):
        exc_type_name = exc_type.__name__
        if exc_type is SyntaxError:
            tbtext = ''.join(traceback.format_exception_only(exc_type, exc_value))
            errmsg = tbtext.replace('File "<string>"', 'File "%s"' % file)
        else:
            errmsg = "Sorry: %s: %s" % (exc_type_name,exc_value)

        Exception.__init__(self,msg or errmsg,exc_type_name,exc_value,file)

        self.exc_type_name = exc_type_name
        self.exc_value = exc_value
        self.file = file
        self.msg = msg or errmsg

    def __str__(self):
        return self.msg


def wr_long(f, x):
    """Internal; write a 32-bit int to a file in little-endian order."""
    f.write(chr( x        & 0xff))
    f.write(chr((x >> 8)  & 0xff))
    f.write(chr((x >> 16) & 0xff))
    f.write(chr((x >> 24) & 0xff))

def compile(file, cfile=None, dfile=None, doraise=False):
    """Does nothing on IronPython.

    IronPython does not currently support compiling to .pyc
    or any other format.
    """
    return # not supported on IronPython, so return unconditionally

def main(args=None):
    """Compile several source files.

    The files named in 'args' (or on the command line, if 'args' is
    not specified) are compiled and the resulting bytecode is cached
    in the normal manner.  This function does not search a directory
    structure to locate source files; it only compiles files named
    explicitly.  If '-' is the only parameter in args, the list of
    files is taken from standard input.

    """
    if args is None:
        args = sys.argv[1:]
    rv = 0
    if args == ['-']:
        while True:
            filename = sys.stdin.readline()
            if not filename:
                break
            filename = filename.rstrip('\n')
            try:
                compile(filename, doraise=True)
            except PyCompileError as error:
                rv = 1
                sys.stderr.write("%s\n" % error.msg)
            except IOError as error:
                rv = 1
                sys.stderr.write("%s\n" % error)
    else:
        for filename in args:
            try:
                compile(filename, doraise=True)
            except PyCompileError as error:
                # return value to indicate at least one failure
                rv = 1
                sys.stderr.write("%s\n" % error.msg)
    return rv

if __name__ == "__main__":
    sys.exit(main())
