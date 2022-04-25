# Local imports

from lib2to3.fixer_base import BaseFix
from lib2to3.fixer_util import Name

#
# Rename AddReferenceToFileAndPath -> AddReference
#

class FixAddReferenceToFileAndPath(BaseFix):

    PATTERN = """
              power< any+ trailer< '.' fixnode='AddReferenceToFileAndPath'> any* >
              """

    def transform(self, node, results):
        fixnode = results['fixnode']
        fixnode.replace(Name('AddReference', prefix=fixnode.prefix))
