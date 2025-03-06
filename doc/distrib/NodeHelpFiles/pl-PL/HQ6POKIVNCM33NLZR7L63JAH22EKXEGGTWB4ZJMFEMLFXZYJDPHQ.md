<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## Informacje szczegółowe
Węzeł `TSplineTopology.BorderVertices` zwraca listę wierzchołków obramowania zawartych w powierzchni T-splajn.

W poniższym przykładzie zostają utworzone dwie powierzchnie T-splajn za pomocą węzła `TSplineSurface.ByCylinderPointsRadius`. Jedna jest powierzchnią otwartą, a druga zostaje pogrubiona za pomocą węzła `TSplineSurface.Thicken`, co przekształca ją w powierzchnię zamkniętą. Gdy obie zostają zbadane za pomocą węzła `TSplineTopology.BorderVertices`, pierwsza zwraca listę wierzchołków obramowania, a druga — pustą listę. Dzieje się tak dlatego, że powierzchnia jest zamknięta, więc nie ma wierzchołków obramowania.
___
## Plik przykładowy

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)
