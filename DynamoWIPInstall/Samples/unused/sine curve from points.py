





import math


doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application 


beginPoint = IN[0]
endPoint = IN[1] 

lineRefPointArray = ReferencePointArray()
lineRefPointArray.Append(beginPoint)
lineRefPointArray.Append(endPoint)

crv = doc.FamilyCreate.NewCurveByPoints(lineRefPointArray)

refptarr = ReferencePointArray()

#use for loop to create a series of points
for i in range(0,20):
    x = i*2
    y = i*2
    #z is controlled using sine
    z = math.sin(i)*2

    myXYZ = XYZ(x,y,z)
    refPt = doc.FamilyCreate.NewReferencePoint(myXYZ)
    refptarr.Append(refPt)

crv2 = doc.FamilyCreate.NewCurveByPoints(refptarr)






import math

doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application

RefPointList1 = IN[0]
RefPointList2 = IN[1]
count = IN[2] 

max = count

#use for loop to connect two series of points
if count > RefPointList1.Size:
    max = RefPointList1.Size

for i in range(0,max):
    pt1 = RefPointList1[i]
    pt2 = RefPointList2[i]
    refptarr.Append(pt1)
    refptarr.Append(pt2)
    crv = doc.FamilyCreate.NewCurveByPoints(refptarr)








import math
 
doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application 


beginPoint = IN[0]
endPoint = IN[1] 

lineRefPointArray = ReferencePointArray()
lineRefPointArray.Append(beginPoint)
lineRefPointArray.Append(endPoint)

crv = doc.FamilyCreate.NewCurveByPoints(lineRefPointArray)
crvRef = crv.GeometryCurve

refptarr = ReferencePointArray()
 
#use for loop to create a series of points
for i in range(0,40):
    pt = crvRef.Evaluate(i,1) # returns and XYZ
    x = 10*pt.X
    y = 10*pt.Y
    #x = i*2
    #y = i*2
    #z is controlled using sine
    z = math.sin(i)*2
 
    myXYZ = XYZ(x,y,z)
    refPt = doc.FamilyCreate.NewReferencePoint(myXYZ)
    refptarr.Append(refPt)
 
crv2 = doc.FamilyCreate.NewCurveByPoints(refptarr)



import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
from Autodesk.Revit.DB import *
import math
doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application
beginPoint = IN[0]
endPoint = IN[1]
lineRefPointArray = ReferencePointArray()
lineRefPointArray.Append(beginPoint)
lineRefPointArray.Append(endPoint)
crv = doc.FamilyCreate.NewCurveByPoints(lineRefPointArray)
crvRef = crv.GeometryCurve
refptarr = ReferencePointArray()
#use for loop to create a series of points
steps = 20
for i in range(0,steps+1):
    pt = crvRef.Evaluate(float(i)/steps,1) # returns and XYZ
    x = pt.X
    y = pt.Y
    #x = i*2
    #y = i*2
    #z is controlled using sine
    z = math.sin(i)*20
    myXYZ = XYZ(x,y,z)
    refPt = doc.FamilyCreate.NewReferencePoint(myXYZ)
    refptarr.Append(refPt)
crv2 = doc.FamilyCreate.NewCurveByPoints(refptarr)


import math

doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application

crv = IN[0]
crvRef = crv.GeometryCurve
refptarr = ReferencePointArray()

#use for loop to create a series of points
steps = 20
for i in range(0,steps+1):
    pt = crvRef.Evaluate(float(i)/steps,1) # returns and XYZ
    x = pt.X
    y = pt.Y
    #x = i*2
    #y = i*2
    #z is controlled using sine
    z = pt.Z + math.sin(i)*20
    myXYZ = XYZ(x,y,z)
    refPt = doc.FamilyCreate.NewReferencePoint(myXYZ)
    refptarr.Append(refPt)
crv2 = doc.FamilyCreate.NewCurveByPoints(refptarr)