""" A Fixer modifies the Parse tree to make code compatible for Python3.
This is a Fixer for keywords None if used in enums.

Usage of x.None is expressed in terms of getattr method:

    x.None -> getattr(x, 'None')

CAVEATS:
1) While the primary target of this fixer is an enum, 
   the fixer will change any x.None usage, regardless of whether it is an enum.
   
Note: Please make sure you do not use a mix of tabs/spaces while editing the code.
"""

# Local imports
from lib2to3 import pytree
from lib2to3.pgen2 import token
from lib2to3 import fixer_base
from lib2to3.fixer_util import Name, Call, Comma, String

# The fixer class needs to derive from BaseFix and
# its name should match that of the .py file.
# Overriding the BaseFix class means implementing an overridden
# 'PATTERN' attribute (similar to AST grammar) you wish to transform 
# and a corresponding 'transform' method. 
class FixNone(fixer_base.BaseFix):
    BM_compatible = True

    PATTERN = """
    anchor=power<
        before=any+
        trailer< '.' 'None' >
    >
    """

    # node is the AST node that is a match with the PATTERN.
    # results is a list of sub-nodes of the matched node.
    def transform(self, node, results):
        assert results
        syms = self.syms
        anchor = results["anchor"]
        # There are 2 types of nodes in the AST - Node and Leaf.
        # Leaf nodes have a prefix and suffix that are meant for whitespaces and comments.
        # It usually suffices to use the prefix only as the prefix of a node is the suffix
        # of its previous node.
        prefix = node.prefix
        # before is the identifier that precedes the '.' before the 'None'.
        before = [n.clone() for n in results["before"]]
        if len(before) == 1:
            before = before[0]
        else:
            before = pytree.Node(syms.power, before)
        noneKeywd = String(repr("None"))
        l_args = [before, Comma(), noneKeywd]
        if l_args:
            l_args[0].prefix = u""
            l_args[2].prefix = u" "
        new = Call(Name(u"getattr"), l_args)
        new.prefix = prefix
        return new
