## Informacje szczegółowe
W poniższym przykładzie zostaje utworzona powierzchnia T-splajn przez wyciągnięcie krzywej NURBS. Za pomocą węzła `TSplineTopology.EdgeByIndex` zostaje wybranych sześć jej krawędzi — trzy po każdej stronie kształtu. Te dwa zestawy krawędzi wraz z powierzchnią zostają przekazane do węzła `TSplineSurface.MergeEdges`. Kolejność grup krawędzi wpływa na kształt — pierwsza grupa krawędzi zostaje przesunięta tak, aby zetknęła się z drugą grupą, która pozostaje w tym samym miejscu. Pozycja danych wejściowych `insertCreases` dodaje opcję fałdowania połączenia wzdłuż scalonych krawędzi. Wynik operacji scalania zostaje przekształcony na potrzeby umieszczenia go z boku w celu zapewnienia lepszego podglądu.
___
## Plik przykładowy

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
