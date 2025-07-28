## In-Depth
Węzeł `TSplineVertex.Index` zwraca numer indeksu wybranego wierzchołka na powierzchni T-splajn. Należy pamiętać, że w topologii powierzchni T-splajn indeksy powierzchni, krawędzi i wierzchołków nie muszą pokrywać się z numerami sekwencji elementów na liście. Aby rozwiązać ten problem, należy użyć węzła `TSplineSurface.CompressIndices`.

W poniższym przykładzie do prymitywu T-splajn w kształcie prostopadłościanu zostaje zastosowany węzeł `TSplineTopology.StarPointVertices`. Następnie za pomocą węzła `TSplineVertex.Index` zostają zbadane indeksy wierzchołków punktów gwiazdowych i węzeł `TSplineTopology.VertexByIndex` zwraca wybrane wierzchołki do dalszej edycji.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
