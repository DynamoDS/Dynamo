## En detalle:
Surface.ByRuledLoft utiliza una lista ordenada de curvas como entrada y soleva una superficie reglada de línea recta entre las curvas. En comparación con ByLoft, ByRuledLoft puede ser ligeramente más rápido, pero la superficie resultante es menos suave. En el siguiente ejemplo, se comienza con una línea a lo largo del eje X. Se traslada esta línea en una serie de líneas que siguen una curva seno en la dirección Y. Mediante esta lista resultante de líneas como entrada para Surface.ByRuledLoft, se obtiene una superficie con segmentos de línea recta entre las curvas de entrada.
___
## Archivo de ejemplo

![ByRuledLoft](./Autodesk.DesignScript.Geometry.Surface.ByRuledLoft_img.jpg)

