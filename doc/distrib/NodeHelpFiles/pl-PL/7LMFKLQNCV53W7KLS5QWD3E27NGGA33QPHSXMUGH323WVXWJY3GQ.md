<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedEdges --->
<!--- 7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ --->
## Informacje szczegółowe
W poniższym przykładzie płaska powierzchnia T-splajn z wyciągniętymi, podzielonymi na składowe i przeciągniętymi wierzchołkami i powierzchniami jest poddawana inspekcji za pomocą węzła `TSplineTopology.DecomposedEdges`, który zwraca listę następujących typów krawędzi zawartych w powierzchni T-splajn:

— `all`: lista wszystkich krawędzi
— `nonManifold`: lista krawędzi nierozmaitościowych
— `border`: lista krawędzi obramowania
— `inner`: lista krawędzi wewnętrznych


Węzeł `Edge.CurveGeometry` służy do wyróżnienia różnych typów krawędzi powierzchni.
___
## Plik przykładowy

![TSplineTopology.DecomposedEdges](./7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ_img.gif)
