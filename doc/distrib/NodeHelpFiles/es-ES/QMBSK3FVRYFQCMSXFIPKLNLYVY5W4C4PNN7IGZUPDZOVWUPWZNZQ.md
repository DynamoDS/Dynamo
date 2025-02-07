<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints` genera una superficie de plano de primitiva de T-Spline a partir de una lista de puntos. Para crear el plano de T-Spline, el nodo utiliza las siguientes entradas:
- `points`: un conjunto de puntos para definir la orientación y el origen del plano. En los casos en los que los puntos de entrada no se encuentran en un único plano, la orientación del plano se determina en función del mejor ajuste. Se requiere un mínimo de tres puntos para crear la superficie.
- `minCorner` y `maxCorner`: las esquinas del plano, representadas como puntos con valores X e Y (se omitirán las coordenadas Z). Estas esquinas representan la extensión de la superficie de T-Spline de salida si se convierte en el plano XY. Los puntos `minCorner` y `maxCorner` no tienen que coincidir con los vértices de las esquinas en 3D.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

En el ejemplo siguiente, se crea una superficie plana de T-Spline mediante una lista de puntos generada aleatoriamente. El tamaño de la superficie se controla mediante los dos puntos utilizados como entradas `minCorner` y `maxCorner`.

## Archivo de ejemplo

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
