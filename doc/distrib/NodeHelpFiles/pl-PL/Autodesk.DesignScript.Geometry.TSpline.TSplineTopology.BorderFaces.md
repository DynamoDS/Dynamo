## Informacje szczegółowe
Węzeł `TSplineTopology.BorderFaces` zwraca listę powierzchni obramowania zawartych w powierzchni T-splajn.

W poniższym przykładzie zostają utworzone dwie powierzchnie T-splajn za pomocą węzła `TSplineSurface.ByCylinderPointsRadius`. Jedna jest powierzchnią otwartą, a druga zostaje pogrubiona za pomocą węzła `TSplineSurface.Thicken`, co przekształca ją w powierzchnię zamkniętą. Gdy obie zostają zbadane za pomocą węzła `TSplineTopology.BorderFaces`, pierwsza zwraca listę powierzchni obramowania, a druga — pustą listę. Dzieje się tak dlatego, że powierzchnia jest zamknięta, więc nie ma powierzchni obramowania.
___
## Plik przykładowy

![TSplineTopology.BorderFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderFaces_img.jpg)
