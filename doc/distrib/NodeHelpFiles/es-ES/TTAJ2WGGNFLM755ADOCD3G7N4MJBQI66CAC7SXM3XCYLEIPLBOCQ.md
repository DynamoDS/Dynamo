<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCoordinateSystemRadii --->
<!--- TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ --->
## In-Depth
En el ejemplo siguiente, se crea una superficie de toroide de T-Spline, con su origen en el sistema de coordenadas, `cs`, especificado. Los radios menor y mayor de la forma se establecen mediante las entradas `innerRadius` y `outerRadius`. Los valores de `innerRadiusSpans` y `outerRadiusSpans` controlan la definición de la superficie a lo largo de las dos direcciones. La simetría inicial de la forma se especifica mediante la entrada `symmetry`. Si la simetría axial aplicada a la forma está activa para el eje X o Y, el valor de `outerRadiusSpans` del toroide debe ser un múltiplo de 4. La simetría radial no presenta este requisito. Por último, la entrada `inSmoothMode` se utiliza para alternar entre la vista preliminar en modo de cuadro y suavizado de la superficie de T-Spline.

## Archivo de ejemplo

![Example](./TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ_img.jpg)
