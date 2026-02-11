## In-Depth
En el ejemplo siguiente, se crea una primitiva de cono de T-Spline mediante el nodo `TSplineSurface.ByConePointsRadius`. La posición y la altura del cono se controlan mediante las dos entradas `startPoint` y `endPoint`. Solo se puede ajustar el radio de base con la entrada `radius`, y el radio superior siempre es cero. `radialSpans` y `heightSpans` determinan los tramos radiales y de altura. La simetría inicial de la forma se especifica mediante la entrada `symmetry`. Si la simetría X o Y se establece en "True" (verdadero), el valor de los tramos radiales debe ser un múltiplo de 4. Por último, la entrada `inSmoothMode` se utiliza para alternar entre la vista preliminar en modo de cuadro y suavizado de la superficie de T-Spline.

## Archivo de ejemplo

![Example](./GVO3NNSNHNAH3DJS5OR37DI2A457QGYX4BQGMHO4IGUUUHZV3HSQ_img.jpg)
