#Parametric Mobius
#Nathan Miller, 2012, The Proving Ground 
#http://nmillerarch.blogspot.com

# Default imports
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
from Autodesk.Revit.DB import *
import Autodesk
import sys
import clr
path = r'C:\Autodesk\Dynamo\Core'
exec_path = r'C:\Users\Ian\Documents\GitHub\Dynamo\bin\AnyCPU\Debug'
sys.path.append(path)
sys.path.append(exec_path)
clr.AddReference('LibGNet')
from Autodesk.LibG import *
import math

doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application

# The __persistent__ dictionary gives you a persistent
# dictionary between runs.  Here we use it to store elements
# we wish to clean up later.

# Initialize if it's not already created.
if 'oldElements' not in __persistent__:
	__persistent__['oldElements'] = []

#delete old elements
if __persistent__['oldElements'].Count > 0:
     count = 0
     for eID in __persistent__['oldElements']:
          doc.Delete( __persistent__['oldElements'][count] )

# Clear the dictionary as we've deleted those old elements
__persistent__['oldElements'] = []
		  
#Declare surface parameters
R = 4.0
T = IN
 
#U and V division variables
uDiv = 6
vDiv = 1
 
#Range variables
u0 = 0
u1 = math.pi
v0 = -1.0
v1 = 1.0
 
#Step increments
uStep = abs(u1 - u0) / uDiv
vStep = abs(v1 - v0) / vDiv
 
#Starting values for u and v
 
#Declare Reference Array Array
refarar = ReferenceArrayArray()
 
#Start value for u
u = 0
while (u <= (u1)):
    #Start value for v
    v = -1
    #Reference Point Array
    refptsarr = ReferencePointArray()
    while (v <= v1):
        #Parametric equations for Mobius plot
        x = (R + v * math.cos(T * u)) * math.cos(u)
        y = (R + v * math.cos(T * u)) * math.sin(u)
        z = v * math.sin(T * u)
 
        #Plot reference points and append to array.
        point = XYZ(x*20,y*20,z*20)
        refpt = doc.FamilyCreate.NewReferencePoint(point)
        refptsarr.Append(refpt)
 
        #Increment v
        v = v + vStep
    #Increment u
    u = u + uStep
 
    #create curve from Reference Point Array
    crv = doc.FamilyCreate.NewCurveByPoints(refptsarr)
 
    #append reference arrays for surface creation
    refarr = ReferenceArray()
    refarr.Append(crv.GeometryCurve.Reference)
    refarar.Append(refarr)
 
#create surface
loft = doc.FamilyCreate.NewLoftForm(True, refarar)
__persistent__['oldElements'].Add(loft.Id)
OUT = loft