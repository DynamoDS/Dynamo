## In-Depth
Należy pamiętać, że w topologii powierzchni T-splajn indeksy elementów `Face`, `Edge` i `Vertex` nie muszą pokrywać się z numerami sekwencji elementów na liście. Aby rozwiązać ten problem, należy użyć węzła `TSplineSurface.CompressIndices`.

W poniższym przykładzie za pomocą węzła `TSplineTopology.DecomposedEdges` zostają pobrane krawędzie obramowania powierzchni T-splajn, a następnie za pomocą węzła `TSplineEdge.Index` zostają pobrane indeksy dostarczonych krawędzi.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
