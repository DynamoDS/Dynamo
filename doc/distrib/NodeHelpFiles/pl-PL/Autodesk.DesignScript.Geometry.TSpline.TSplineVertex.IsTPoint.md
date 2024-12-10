## In-Depth
Węzeł `TSplineVertex.IsTPoint` zwraca informację o tym, czy wierzchołek jest punktem T. Punkty T są wierzchołkami na końcu częściowych wierszy punktów sterujących.

W poniższym przykładzie węzeł `TSplineSurface.SubdivideFaces` przetwarza prymityw prostopadłościanowy T-splajn w ramach przykładu jednego z wielu sposobów dodawania punktów T do powierzchni. Węzeł `TSplineVertex.IsTPoint` potwierdza, że wierzchołek o danym indeksie jest punktem T. Aby zapewnić lepszą wizualizację położenia punktów T, używane są węzły `TSplineVertex.UVNFrame` i `TSplineUVNFrame.Position`.



## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
