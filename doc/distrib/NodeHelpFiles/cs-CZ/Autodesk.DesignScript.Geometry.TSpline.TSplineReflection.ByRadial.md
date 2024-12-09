## In-Depth
Uzel `TSplineReflection.ByRadial` vrací objekt `TSplineReflection`, který je možné použít jako vstup pro uzel `TSplineSurface.AddReflections`. Uzel přijímá rovinu jako vstup a normála roviny slouží jako osa k otočení geometrie. Podobně jako objekt TSplineInitialSymmetry i objekt TSplineReflection ovlivňuje všechny další operace a úpravy, jakmile je pro objekt TSplineSurface vytvořen.

V níže uvedeném příkladu je pomocí uzlu `TSplineReflection.ByRadial` definován odraz povrchu T-Spline. Vstupy `segmentsCount` a `segmentAngle` určují způsob, jakým se geometrie odráží kolem normály dané roviny. Výstup uzlu se poté použije jako vstup pro uzel `TSplineSurface.AddReflections` k vytvoření nového povrchu T-Spline.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
