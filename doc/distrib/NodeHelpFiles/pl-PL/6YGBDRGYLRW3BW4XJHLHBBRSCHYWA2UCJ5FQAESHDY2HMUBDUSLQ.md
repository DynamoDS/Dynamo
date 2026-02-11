<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
Węzeł `TSplineSurface.AddReflections` tworzy nową powierzchnię T-splajn przez zastosowanie jednego lub wielu odbić do wejściowej powierzchni `tSplineSurface`. Wejściowa wartość logiczna (Boolean) `weldSymmetricPortions` określa, czy pofałdowane krawędzie wygenerowane przez odbicie są wygładzane, czy zachowywane.

Poniższy przykład ilustruje, jak dodać wiele odbić do powierzchni T-splajn za pomocą węzła `TSplineSurface.AddReflections`. Utworzone zostają dwa odbicia: osiowe i promieniowe. Geometria bazowa jest powierzchnią T-splajn w kształcie przeciągnięcia ze ścieżką biegnącą po łuku. Te dwa odbicia zostają połączone na liście i służą jako dane wejściowe węzła `TSplineSurface.AddReflections` wraz z geometrią bazową do odbicia. Powierzchnie TSplineSurfaces zostają złączone, co daje wygładzoną powierzchnię TSplineSurface bez pofałdowanych krawędzi. Powierzchnia jest dodatkowo zmieniana przez przesunięcie jednego wierzchołka za pomocą węzła `TSplineSurface.MoveVertex`. Ze względu na to, że odbicie zostaje zastosowane do powierzchni T-splajn, ruch wierzchołka jest odtwarzany 16 razy.

## Plik przykładowy

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
