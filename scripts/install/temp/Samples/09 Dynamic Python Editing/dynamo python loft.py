# adapted from Nathan Miller's Proving Ground Blog
# http://theprovingground.wikidot.com/revit-api-py-forms

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

scale = IN
# *scale

if __persistent__.ContainsKey("elements"):
    for eID in __persistent__["elements"]:
        e = doc.get_Element(eID)
        doc.Delete(e)
else:
		__persistent__["elements"] = []

refarr = ReferenceArray()
refarrarr = ReferenceArrayArray()
 
#Create first profile curve
refptarr1 = ReferencePointArray()
pt1 = XYZ(0,0,5*scale/2)
pt2 = XYZ(20,0,-5)
pt3 = XYZ(40,0,5*scale/2)
refptarr1.Append(doc.FamilyCreate.NewReferencePoint(pt1))
refptarr1.Append(doc.FamilyCreate.NewReferencePoint(pt2))
refptarr1.Append(doc.FamilyCreate.NewReferencePoint(pt3))
crv1 = doc.FamilyCreate.NewCurveByPoints(refptarr1)
 
#Append reference arrays
refarr1 = ReferenceArray()
refarr1.Append(crv1.GeometryCurve.Reference)
refarrarr.Append(refarr1)
 
#Create second profile curve
refptarr2 = ReferencePointArray()
pt4 = XYZ(0,20,0)
pt5 = XYZ(20,20,5*scale)
pt6 = XYZ(40,20,0)
refptarr2.Append(doc.FamilyCreate.NewReferencePoint(pt4))
refptarr2.Append(doc.FamilyCreate.NewReferencePoint(pt5))
refptarr2.Append(doc.FamilyCreate.NewReferencePoint(pt6))
crv2 = doc.FamilyCreate.NewCurveByPoints(refptarr2)
 
#Append reference arrays
refarr2 = ReferenceArray()
refarr2.Append(crv2.GeometryCurve.Reference)
refarrarr.Append(refarr2)
 
#Create third profile curve
refptarr3 = ReferencePointArray()
pt7 = XYZ(0,40,5*scale/2)
pt8 = XYZ(20,40,-5)
pt9 = XYZ(40,40,5*scale/2)
refptarr3.Append(doc.FamilyCreate.NewReferencePoint(pt7))
refptarr3.Append(doc.FamilyCreate.NewReferencePoint(pt8))
refptarr3.Append(doc.FamilyCreate.NewReferencePoint(pt9))
crv3 = doc.FamilyCreate.NewCurveByPoints(refptarr3)
 
#Append reference arrays
refarr3 = ReferenceArray()
refarr3.Append(crv3.GeometryCurve.Reference)
refarrarr.Append(refarr3)
 
#create Loft
loft = doc.FamilyCreate.NewLoftForm(True, refarrarr)
__persistent__["elements"].Add(loft.Id)
OUT = loft