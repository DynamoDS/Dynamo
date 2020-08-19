# Copyright 2006 Google, Inc. All Rights Reserved.
# Licensed to PSF under a Contributor Agreement.

"""Fixer for keywords None if used in enums.

Usage of x.None is expressed in terms of getattr method:

    x.None -> getattr(x, 'None')

CAVEATS:
1) While the primary target of this fixer is an enum, 
   the fixer will change any x.None usage, regardless of whether it is an enum.
"""

# Local imports
from lib2to3 import pytree
from lib2to3.pgen2 import token
from lib2to3 import fixer_base
from lib2to3.fixer_util import Name, Call, Comma, String


class FixNone(fixer_base.BaseFix):
    BM_compatible = True

    PATTERN = """
    anchor=power<
        before=any+
        trailer< '.' 'None' >
    >
    """

    def transform(self, node, results):
        assert results
        syms = self.syms
        anchor = results["anchor"]
        prefix = node.prefix
        before = [n.clone() for n in results["before"]]
        if len(before) == 1:
            before = before[0]
        else:
            before = pytree.Node(syms.power, before)
        before.prefix = u" "
        l_args = [before, Comma(), String(repr("None"))]
        if l_args:
            l_args[0].prefix = u""
            l_args[2].prefix = u""
        new = Call(Name(u"getattr"), l_args)
        new.prefix = prefix
        return new
