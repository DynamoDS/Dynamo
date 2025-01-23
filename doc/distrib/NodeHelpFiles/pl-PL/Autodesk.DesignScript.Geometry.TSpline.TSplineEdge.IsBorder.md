## In-Depth
Węzeł `TSplineEdge.IsBorder` zwraca wartość `True` (Prawda), jeśli wejściowa krawędź T-splajn jest częścią obramowania.

W poniższym przykładzie zostają zbadane krawędzie dwóch powierzchni T-splajn. Te powierzchnie to walec i jego wersja pogrubiona. Aby wybrać wszystkie krawędzie, w obu przypadkach używane są węzły `TSplineTopology.EdgeByIndex` z wejściem indices, na którym zostaje podany zakres liczb całkowitych od 0 do n, gdzie n jest liczbą krawędzi podawaną z węzła `TSplineTopology.EdgesCount`. Jest to rozwiązanie alternatywne wobec bezpośredniego wybrania krawędzi za pomocą węzła `TSplineTopology.DecomposedEdges`. W przypadku walca pogrubionego jest dodatkowo używany węzeł `TSplineSurface.CompressIndices` do zmiany kolejności indeksów krawędzi.
Węzeł `TSplineEdge.IsBorder` sprawdza, które krawędzie są krawędziami obramowania. Położenie krawędzi obramowania walca płaskiego jest wyróżnione za pomocą węzłów `TSplineEdge.UVNFrame` i `TSplineUVNFrame.Position`. Walec pogrubiony nie ma krawędzi obramowania.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
