## In-Depth
Właściwość UVNFrame powierzchni dostarcza przydatnych informacji na temat położenia i orientacji powierzchni poprzez zwrócenie wektora normalnego i kierunków UV.
W poniższym przykładzie węzeł `TSplineFace.UVNFrame` służy do zwizualizowania rozkładu powierzchni na prymitywie kuli kwadrantowej. Za pomocą węzła `TSplineTopology.DecomposedFaces` zostają zbadane wszystkie powierzchnie, a następnie za pomocą węzła `TSplineFace.UVNFrame` zostają pobrane położenia centroid powierzchni jako punkty. Te punkty zostają zwizualizowane przy użyciu węzła `TSplineUVNFrame.Position`. Etykiety są wyświetlane w podglądzie w tle przez włączenie opcji pokazania etykiet w menu kontekstowym węzła.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
