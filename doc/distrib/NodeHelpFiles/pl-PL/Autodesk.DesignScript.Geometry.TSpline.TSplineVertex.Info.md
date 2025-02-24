## In-Depth
Węzeł `TSplineVertex.Info` zwraca następujące właściwości wierzchołka T-splajn:
— `uvnFrame`: punkt na powłoce, wektor U, wektor V i wektor normalny wierzchołka T-splajn
— `index`: indeks wybranego wierzchołka na powierzchni T-splajn
— `isStarPoint`: informacja o tym, czy wybrany wierzchołek jest punktem gwiazdowym
— `isTpoint`: informacja o tym, czy wybrany wierzchołek jest punktem T
— `isManifold`: informacja o tym, czy wybrany wierzchołek jest rozmaitościowy
— `valence`: liczba krawędzi w wybranym wierzchołku T-splajn
— `functionalValence`: funkcjonalny stopień (valence) wierzchołka. Więcej informacji podano w dokumentacji węzła `TSplineVertex.FunctionalValence`.

W poniższym przykładzie za pomocą węzłów `TSplineSurface.ByBoxCorners` i `TSplineTopology.VertexByIndex` zostają odpowiednio utworzona powierzchnia T-splajn i wybrane jej wierzchołki. Za pomocą węzła `TSplineVertex.Info` zostają zebrane powyższe informacje o wybranym wierzchołku.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
