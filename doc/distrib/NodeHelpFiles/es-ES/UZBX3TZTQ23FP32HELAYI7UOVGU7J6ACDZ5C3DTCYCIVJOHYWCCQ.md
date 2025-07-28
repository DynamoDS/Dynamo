<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## En detalle:
`TSplineSurface.BuildFromLines` permite crear una superficie de T-Spline más compleja que puede utilizarse como geometría final o como una primitiva personalizada que se aproxime más a la forma deseada que las primitivas por defecto. El resultado puede ser una superficie cerrada o abierta y puede tener agujeros o aristas plegadas.

La entrada del nodo es una lista de curvas que representan una "jaula de control" para la superficie de T-Spline. La configuración de la lista de líneas requiere cierta preparación y debe seguir determinadas pautas.
- Las líneas no deben solaparse.
- El borde del polígono debe estar cerrado y cada punto final de línea debe encontrarse al menos con otro punto final. Cada intersección de línea debe encontrarse en un punto.
- Se requiere una densidad mayor de los polígonos para las áreas con mayor detalle.
- Los cuadrantes son preferibles a los triángulos y los segmentos porque son más fáciles de controlar.

En el ejemplo siguiente, se crean dos superficies de T-Spline para ilustrar el uso de este nodo. `maxFaceValence` se deja en el valor por defecto para ambos casos y `snappingTolerance` se ajusta para asegurar que las líneas dentro del valor de tolerancia se traten como uniones. Para la forma de la izquierda, `creaseOuterVertices` se ajusta en "False" (falso) a fin de mantener dos vértices de esquina afilados y no redondeados. La forma de la izquierda no presenta vértices exteriores y esta entrada se deja en el valor por defecto. `inSmoothMode` se activa para ambas formas a fin de obtener una vista preliminar suavizada.

___
## Archivo de ejemplo

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
