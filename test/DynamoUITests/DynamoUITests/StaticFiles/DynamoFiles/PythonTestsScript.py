# Load the Python Standard and DesignScript Libraries
import sys
import clr

clr.AddReference('ProtoGeometry')
from Autodesk.DesignScript.Geometry import *

# The inputs to this node will be stored as a list in the IN variables.
t = IN[0]

# Place your code below this line

# Assign your output to the OUT variable.
OUT = t * 5