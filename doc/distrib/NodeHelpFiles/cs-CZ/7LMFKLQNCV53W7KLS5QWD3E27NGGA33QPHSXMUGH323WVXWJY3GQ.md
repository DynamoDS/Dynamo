<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedEdges --->
<!--- 7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ --->
## Podrobnosti
V níže uvedeném příkladu je rovinný povrch T-Spline s vysunutými, dále rozdělenými a taženými vrcholy a plochami zkontrolován pomocí uzlu `TSplineTopology.DecomposedEdges`, který vrací seznam následujících typů hran obsažených v povrchu T-Spline:

- `all`: seznam všech hran
- `nonManifold`: seznam nerozložených hran
- `border`: seznam okrajových hran
- `inner`: seznam vnitřních hran


Uzel `Edge.CurveGeometry` slouží ke zvýraznění různých typů hran povrchu.
___
## Vzorový soubor

![TSplineTopology.DecomposedEdges](./7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ_img.gif)
