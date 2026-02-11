## In-Depth
Ten węzeł zwraca obiekt TSplineUVNFrame, który może być przydatny do wizualizowania położenia i orientacji wierzchołka oraz do dalszego manipulowania powierzchnią T-splajn przy użyciu wektorów U, V lub N.

W poniższym przykładzie za pomocą węzła `TSplineVertex.UVNFrame` zostaje pobrana ramka UVN wybranego wierzchołka. Następnie na podstawie ramki UVN zostaje zwrócony wektor normalny wierzchołka. Na koniec za pomocą kierunku wektora normalnego wierzchołek zostaje przesunięty przy użyciu węzła `TSplineSurface.MoveVertices`.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
