<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## Podrobnosti
V níže uvedeném příkladu je rovinný povrch T-Spline s vysunutými, dále rozdělenými a taženými vrcholy a plochami zkontrolován pomocí uzlu `TSplineTopology.DecomposedVertices`, který vrací seznam následujících typů vrcholů obsažených v povrchu T-Spline:

- `all`: seznam všech vrcholů
- `regular`: seznam běžných vrcholů
- `tPoints`: seznam vrcholů bodů T
- `starPoints`: seznam vrcholů cípů hvězdy
- `nonManifold`: seznam nerozložených vrcholů
- `border`: seznam okrajových vrcholů
- `inner`: seznam vnitřních vrcholů

Uzly `TSplineVertex.UVNFrame` a `TSplineUVNFrame.Position` slouží ke zvýraznění různých typů vrcholů povrchu.

___
## Vzorový soubor

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
