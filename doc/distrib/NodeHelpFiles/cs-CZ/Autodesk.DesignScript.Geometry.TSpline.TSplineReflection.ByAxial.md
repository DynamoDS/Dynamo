## In-Depth
Uzel `TSplineReflection.ByAxial` vrací objekt `TSplineReflection`, který je možné použít jako vstup pro uzel `TSplineSurface.AddReflections`.
Vstup uzlu `TSplineReflection.ByAxial` je rovina, která slouží jako rovina zrcadlení. Podobně jako objekt TSplineInitialSymmetry i objekt TSplineReflection ovlivňuje všechny další operace a úpravy, jakmile je pro objekt TSplineSurface vytvořen.

V níže uvedeném příkladu je pomocí uzlu `TSplineReflection.ByAxial` vytvořen objekt TSplineReflection umístěný v horní části kužele T-Spline. Poté se jako vstup uzlu `TSplineSurface.AddReflections` použije odraz, který zobrazuje kužel a vrací nový povrch T-Spline.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
