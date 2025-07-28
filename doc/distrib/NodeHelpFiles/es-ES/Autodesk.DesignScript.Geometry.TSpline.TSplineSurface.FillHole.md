## En detalle:
En el ejemplo siguiente, los huecos de una superficie cilíndrica de T-Spline se rellenan mediante el nodo `TSplineSurface.FillHole`, que requiere las siguientes entradas:
- `edges`: un número de aristas de borde seleccionadas de la superficie de T-Spline que se va a rellenar.
- `fillMethod`: un valor numérico de 0 a 3 que indica el método de llenado:
    * 0 rellena el agujero con triangulación.
    * 1 rellena el agujero con una sola cara de NGon.
    * 2 crea un punto en el centro del agujero desde el que las caras triangulares se irradian hacia las aristas.
    * 3 es similar al método 2 con la diferencia de que los vértices centrales se sueldan en un vértice en lugar de apilarse encima.
- `keepSubdCreases`: un valor booleano que indica si se conservan los pliegues subdivididos.
___
## Archivo de ejemplo

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
