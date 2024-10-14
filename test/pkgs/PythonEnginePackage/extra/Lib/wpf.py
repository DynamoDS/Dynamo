#-*- coding: ISO-8859-1 -*-

def _():
    import sys
    if sys.platform == 'cli':
        import clr
        try:
            clr.AddReference('IronPython.Wpf')
        except:
            pass
_()
del _

from _wpf import *
