## In profondità
`TSplineTopology.BorderEdges` restituisce un elenco di bordi contenuti nella superficie T-Spline.

Nell'esempio seguente, due superfici T-Spline vengono create mediante `TSplineSurface.ByCylinderPointsRadius`; una è una superficie aperta mentre l'altra è ispessita utilizzando `TSplineSurface.Thicken`, che la trasforma in una superficie chiusa. Quando entrambe le superfici vengono esaminate con il nodo `TSplineTopology.BorderEdges`, la prima restituisce un elenco di bordi mentre la seconda restituisce un elenco vuoto. Questo perché, essendo la superficie racchiusa, non ci sono bordi.
___
## File di esempio

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)
