<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
La valencia funcional de un vértice va más allá de un simple recuento de aristas adyacentes y tiene en cuenta las líneas de rejilla virtuales que influyen en la combinación del vértice en el área que lo rodea. Proporciona una comprensión más matizada de cómo los vértices y sus aristas influyen en la superficie durante las operaciones de deformación y perfeccionamiento.
Cuando se utiliza en vértices normales y puntos T, el nodo `TSplineVertex.FunctionalValence` devuelve el valor "4", lo que significa que la superficie está guiada por splines en forma de rejilla. Una valencia funcional distinta a "4" indica que el vértice es un punto de estrella y la fusión alrededor del vértice será menos suave.

En el ejemplo siguiente, se utiliza `TSplineVertex.FunctionalValence` en dos vértices de puntos T de una superficie plana de T-Spline. El nodo `TSplineVertex.Valence` devuelve el valor 3, mientras que la valencia funcional de los vértices seleccionados es 4, que es específica de los puntos T. Los nodos `TSplineVertex.UVNFrame` y `TSplineUVNFrame.Position` se utilizan para visualizar la posición de los vértices analizados.

## Archivo de ejemplo

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
