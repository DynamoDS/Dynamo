## In-Depth
En el ejemplo siguiente, se crea una superficie de T-Spline como extrusión de una determinada curva de perfil. La curva puede ser abierta o cerrada. La extrusión se realiza en la dirección especificada y puede estar controlada en ambas direcciones por las entradas `frontDistance` y `backDistance`. Los tramos se pueden definir individualmente para las dos direcciones de extrusión, con los valores de `frontSpans` y `backSpans` especificados. Para establecer la definición de la superficie a lo largo de la curva, `profileSpans` controla el número de caras y `uniform` las distribuye de manera uniforme o tiene en cuenta la curvatura. Por último, `inSmoothMode` controla si la superficie se muestra en modo de cuadro o suavizado.

## Archivo de ejemplo
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
