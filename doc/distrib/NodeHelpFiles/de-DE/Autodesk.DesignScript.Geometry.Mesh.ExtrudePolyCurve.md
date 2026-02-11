## Im Detail
Der Block `Mesh.ExtrudePolyCurve` extrudiert eine bereitgestellte `polycurve` um einen bestimmten Abstand, der durch die Eingabe für `height` festgelegt wurde, und in die angegebene Vektorrichtung. Offene Polykurven werden geschlossen, indem der erste mit dem letzten Punkt verbunden wird. Wenn die anfängliche `polycurve` planar ist und sich nicht selbst schneidet, kann das resultierende Netz abgeschlossen werden, um ein Volumennetz zu bilden.
Im folgenden Beispiel wird `Mesh.ExtrudePolyCurve` verwendet, um ein geschlossenes Netz basierend auf einer geschlossenen Polykurve zu erstellen.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.ExtrudePolyCurve_img.jpg)
