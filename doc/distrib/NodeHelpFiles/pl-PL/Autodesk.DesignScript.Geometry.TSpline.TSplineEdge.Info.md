## In-Depth
Węzeł `TSplineEdge.Info` zwraca następujące właściwości krawędzi powierzchni T-splajn:
— `uvnFrame`: punkt na powłoce, wektor U, wektor V i wektor normalny krawędzi T-splajn
— `index`: indeks krawędzi
— `isBorder`: informacja o tym, czy wybrana krawędź jest częścią obramowania powierzchni T-splajn
— `isManifold`: informacja o tym, czy wybrana krawędź jest rozmaitościowa

W poniższym przykładzie za pomocą węzła `TSplineTopology.DecomposedEdges` zostaje pobrana lista wszystkich krawędzi powierzchni prymitywu walcowego T-splajn, a za pomocą węzła `TSplineEdge.Info` zostają zbadane ich właściwości.


## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
