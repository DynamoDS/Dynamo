## In-Depth
W poniższym przykładzie zostaje utworzona powierzchnia nierozmaitościowa przez połączenie dwóch powierzchni, które mają wspólną krawędź wewnętrzną. Wynikiem jest powierzchnia, która nie ma wyraźnego przodu i tyłu. Powierzchnia nierozmaitościowa może być wyświetlana tylko w trybie ramki, dopóki nie zostanie naprawiona. Za pomocą węzła `TSplineTopology.DecomposedVertices` zostają zbadane wszystkie wierzchołki powierzchni, a za pomocą węzła `TSplineVertex.IsManifold` zostają wyróżnione wierzchołki kwalifikujące się jako rozmaitościowe. Wierzchołki nierozmaitościowe zostają wyodrębnione, a ich położenie zostaje zwizualizowane za pomocą węzłów `TSplineVertex.UVNFrame` i `TSplineUVNFrame.Position`.


## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)
