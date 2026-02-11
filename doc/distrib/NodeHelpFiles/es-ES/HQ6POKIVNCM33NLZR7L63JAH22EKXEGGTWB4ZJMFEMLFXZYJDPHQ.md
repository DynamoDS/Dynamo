<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## En detalle:
`TSplineTopology.BorderVertices` devuelve una lista de vértices de borde incluidos en una superficie de T-Spline.

En el ejemplo siguiente, se crean dos superficies de T-Spline mediante `TSplineSurface.ByCylinderPointsRadius`. Una es una superficie abierta, mientras que la otra se engrosa mediante `TSplineSurface.Thicken`, lo que la convierte en una superficie cerrada. Cuando se examinan ambas con el nodo `TSplineTopology.BorderVertices`, la primera devuelve una lista de vértices de borde, mientras que la segunda devuelve una lista vacía. Esto se debe a que, como se trata de una superficie cerrada, no hay vértices de borde.
___
## Archivo de ejemplo

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)
