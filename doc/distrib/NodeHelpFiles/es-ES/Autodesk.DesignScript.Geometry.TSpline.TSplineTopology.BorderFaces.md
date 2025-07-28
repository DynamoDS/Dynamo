## En detalle:
`TSplineTopology.BorderFaces` devuelve una lista de caras de borde incluidas en la superficie de T-Spline.

En el ejemplo siguiente, se crean dos superficies de T-Spline mediante `TSplineSurface.ByCylinderPointsRadius`. Una es una superficie abierta, mientras que la otra se engrosa mediante `TSplineSurface.Thicken`, lo que la convierte en una superficie cerrada. Cuando se examinan ambas con el nodo `TSplineTopology.BorderFaces`, la primera devuelve una lista de caras de borde mientras que la segunda devuelve una lista vacía. Esto se debe a que, como se trata de una superficie cerrada, no hay caras de borde.
___
## Archivo de ejemplo

![TSplineTopology.BorderFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderFaces_img.jpg)
