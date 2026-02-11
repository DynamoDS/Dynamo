## Informacje szczegółowe
W poniższym przykładzie za pomocą węzła `TSplineTopology.VertexByIndex` zostaje pobrany wierzchołek powierzchni T-splajn. Wierzchołek służy następnie jako dane wejściowe węzła `TSplineSurface.MoveVertices`. Wierzchołek zostaje przesunięty w kierunku określonym przez pozycję danych wejściowych `vector`. Pozycja `onSurface` w razie ustawienia `True` (Prawda) powoduje uwzględnienie powierzchni przy przesunięciu, a w razie wartości `False` (Fałsz) ruch jest oparty na punktach sterowania.
___
## Plik przykładowy

![TSplineSurface.MoveVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MoveVertices_img.jpg)
