<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## Informacje szczegółowe
W poniższym przykładzie płaska powierzchnia T-splajn z wyciągniętymi, podzielonymi na składowe i przeciągniętymi wierzchołkami i powierzchniami jest poddawana inspekcji za pomocą węzła `TSplineTopology.DecomposedVertices`, który zwraca listę następujących typów wierzchołków zawartych w powierzchni T-splajn:

— `all`: lista wszystkich wierzchołków
— `regular`: lista zwykłych wierzchołków
— `tPoints`: lista wierzchołków w punktach T
— `starPoints`: lista wierzchołków w punktach gwiazdowych
— `nonManifold`: lista wierzchołków nierozmaitościowych
— `border`: lista wierzchołków obramowania
— `inner`: lista wierzchołków wewnętrznych

Węzły `TSplineVertex.UVNFrame` i `TSplineUVNFrame.Position` służą do wyróżnienia różnych typów wierzchołków powierzchni.

___
## Plik przykładowy

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
