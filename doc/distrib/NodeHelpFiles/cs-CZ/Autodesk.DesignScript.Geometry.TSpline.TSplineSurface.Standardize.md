## Podrobnosti
Uzel `TSplineSurface.Standardize` slouží ke standardizaci povrchu T-Spline.
Standardizací se rozumí příprava povrchu T-Spline na převod na NURBS a prodlužování všech bodů T, dokud nebudou odděleny od cípů hvězdy nejméně dvěma izokřivkami. Standardizací se nezmění tvar povrchu, ale mohou se přidat řídicí body, aby povrch splňoval požadavky geometrie nezbytné k zajištění kompatibility povrchu s NURBS.

V níže uvedeném příkladu má povrch T-Spline vytvořený pomocí uzlu `TSplineSurface.ByBoxLengths` jednu ze svých ploch rozdělenou.
Uzel `TSplineSurface.IsStandard` slouží ke kontrole, zda je povrch standardní, je však vrácen záporný výsledek.
Poté se povrch standardizuje pomocí uzlu `TSplineSurface.Standardize`. Výsledný povrch se zkontroluje pomocí uzlu `TSplineSurface.IsStandard`, který potvrzuje, že povrch je nyní standardní.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Vzorový soubor

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
