<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## In profondità
`TSplineTopology.BorderVertices` restituisce un elenco di vertici dei bordi contenuti in una superficie T-Spline.

Nell'esempio seguente, due superfici T-Spline vengono create mediante `TSplineSurface.ByCylinderPointsRadius`. Una è una superficie aperta mentre l'altra è ispessita utilizzando `TSplineSurface.Thicken`, che la trasforma in una superficie chiusa. Quando entrambe le superfici vengono esaminate con il nodo `TSplineTopology.BorderVertices`, la prima restituisce un elenco di vertici dei bordi mentre la seconda restituisce un elenco vuoto. Questo perché, essendo la superficie racchiusa, non ci sono vertici dei bordi.
___
## File di esempio

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)
