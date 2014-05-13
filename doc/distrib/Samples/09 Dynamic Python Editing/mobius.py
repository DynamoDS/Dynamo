#Parametric Mobius
import clr
clr.AddReference("ProtoGeometry")

import math
from Autodesk.DesignScript.Geometry import Surface, NurbsCurve, Point

def frange(start, end=None, inc=None):
    "A range function that accepts float increments..."

    if not end:
        end, start = start + 0.0, 0.0
    else: 
        start += 0.0 # force it to be a float

    if not inc:
        inc = 1.0

    count = int(math.ceil((end - start) / inc)) + 1
    absend = abs(end)
    return (x for x in (start + i*inc for i in xrange(count)) if abs(x) <= absend)


#Declare surface parameters
T = IN[0]
R = 4.0

#U and V division variables
uDiv = 6
vDiv = 1

#Range variables
u0 = 0
u1 = math.pi*2
v0 = -1.0
v1 = 1.0

#Step increments
uStep = abs(u1 - u0) / uDiv
vStep = abs(v1 - v0) / vDiv

OUT = Surface.ByLoft(
    [NurbsCurve.ByPoints(
        [Point.ByCoordinates(
            (R + v * math.cos(T * u)) * math.cos(u) * 20,
            (R + v * math.cos(T * u)) * math.sin(u) * 20,
            v * math.sin(T * u) * 20)
        for v in frange(v0, v1, vStep)])
    for u in frange(u0, u1, uStep)])
