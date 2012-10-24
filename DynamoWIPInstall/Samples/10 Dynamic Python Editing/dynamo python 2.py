scale = IN

 
doc = __revit__.ActiveUIDocument.Document
app = __revit__.Application
if DynStoredElements.Count>0:
     __dynamo__.TryGetElement(DynStoredElements[0], e)
refarr = ReferenceArray()
refarrarr = ReferenceArrayArray()
 
#Create first profile curve
refptarr1 = ReferencePointArray()
pt1 = XYZ(0,0,5*scale)
pt2 = XYZ(20,0,-5)
pt3 = XYZ(40,0,5)
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
pt5 = XYZ(20,20,10)
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
pt7 = XYZ(0,40,5)
pt8 = XYZ(20,40,-5)
pt9 = XYZ(40,40,5)
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
DynStoredElements.Add(loft.Id)
#OUT = loft