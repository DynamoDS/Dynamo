## Podrobnosti
Povrch T-Spline je standardní, pokud jsou všechny body T odděleny od cípů hvězdy alespoň dvěma izokřivkami. Standardizace je nutná k převodu povrchu T-Spline na povrch NURBS.

V níže uvedeném příkladu má povrch T-Spline vygenerovaný uzlem `TSplineSurface.ByBoxLengths` jednu z jeho ploch dále rozdělenou. Pomocí uzlu `TSplineSurface.IsStandard` se zkontroluje, zda je povrch standardní, ale výsledek je negativní.
Poté se povrch standardizuje pomocí uzlu `TSplineSurface.Standardize`. Budou zavedeny nové řídicí body, aniž by se změnil tvar povrchu. Výsledný povrch bude zkontrolován pomocí uzlu `TSplineSurface.IsStandard`, která potvrdí, že je povrch nyní standardní.
Uzly `TSplineFace.UVNFrame` a `TSplineUVNFrame.Position` slouží ke zvýraznění rozdělené plochy v povrchu.
___
## Vzorový soubor

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
