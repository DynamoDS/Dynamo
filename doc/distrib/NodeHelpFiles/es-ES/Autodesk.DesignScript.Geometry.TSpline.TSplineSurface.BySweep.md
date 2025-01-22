## En detalle:
En el ejemplo siguiente, se crea una superficie de T-Spline mediante el barrido de un perfil en torno a una ruta especificada. La entrada `parallel` controla si los tramos del perfil permanecen paralelos a la dirección de la ruta o si se rotan a lo largo de ella. La definición de la forma se establece mediante `pathSpans` y `radialSpans`. La entrada `pathUniform` define si los tramos de la ruta se distribuyen uniformemente o teniendo en cuenta la curvatura. Un parámetro similar, `profileUniform`, controla los tramos a lo largo del perfil. La simetría inicial de la forma se especifica mediante la entrada `symmetry`. Por último, la entrada `inSmoothMode` se utiliza para alternar entre la vista preliminar en modo de cuadro y suavizado de la superficie de T-Spline.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySweep_img.jpg)
