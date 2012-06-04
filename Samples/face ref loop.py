import math
import clr
clr.AddReference('RevitAPI') 
clr.AddReference('RevitAPIUI') 
from Autodesk.Revit.DB import * 
 
doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application 
 

#IN is a FaceRef from a form
 
refptarr = ReferencePointArray()
 
#use for loop to create a series of points
for i in range(0,10):
    u = float(i)/100
    for j in range(0,10):
        v = float(j)/100
        myUV = UV(u,v)
        # PointElementReference facePoint = this.UIDocument.Application.Application.Create.NewPointOnFace(r, myUV);# //C sharp code
        facePoint = app.Create.NewPointOnFace(IN,myUV)
        refPt = doc.FamilyCreate.NewReferencePoint(facePoint)
        refptarr.Append(refPt) 
        OUT = refptarr



import math
import clr
clr.AddReference('RevitAPI') 
clr.AddReference('RevitAPIUI') 
from Autodesk.Revit.DB import * 
 
doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application 
 

#IN is a FaceRef from a form
 
refptarr = ReferencePointArray()
 

myUV = UV(.5,.5)
# PointElementReference facePoint = this.UIDocument.Application.Application.Create.NewPointOnFace(r, myUV);# //C sharp code
facePoint = app.Create.NewPointOnFace(IN,myUV)
refPt = doc.FamilyCreate.NewReferencePoint(facePoint)
OUT = refPt
