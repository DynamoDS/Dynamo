<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
W poniższym przykładzie węzeł `TSplineSurface.UncreaseVertices` zostaje zastosowany do wierzchołków narożników prymitywu płaszczyznowego. Domyślnie wierzchołki te są fałdowane w momencie utworzenia powierzchni. Wierzchołki zostają zidentyfikowane za pomocą węzłów `TSplineVertex.UVNFrame` i `TSplineUVNFrame.Poision` z włączoną opcją `Show Labels`. Wierzchołki narożników zostają następnie wybrane przy użyciu węzła `TSplineTopology.VertexByIndex` i fałdowanie zostaje usunięte. Efekt tej operacji można obejrzeć, jeśli kształt jest w trybie podglądu gładkiego.

## Plik przykładowy

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
