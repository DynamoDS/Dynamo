## Informacje szczegółowe
Węzeł `TSplineTopology.BorderEdges` zwraca listę krawędzi obramowania zawartych w powierzchni T-splajn.

W poniższym przykładzie zostają utworzone dwie powierzchnie T-splajn za pomocą węzła `TSplineSurface.ByCylinderPointsRadius`; jedna jest powierzchnią otwartą, a druga zostaje pogrubiona za pomocą węzła `TSplineSurface.Thicken`, co przekształca ją w powierzchnię zamkniętą. Gdy obie zostają zbadane za pomocą węzła `TSplineTopology.BorderEdges`, pierwsza zwraca listę krawędzi obramowania, a druga — pustą listę. Dzieje się tak dlatego, że powierzchnia jest zamknięta, więc nie ma krawędzi obramowania.
___
## Plik przykładowy

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)
