## In-Depth

En el ejemplo siguiente, la operación de anulación de la soldadura se realiza en una fila de aristas de una superficie de T-Spline. Como resultado, los vértices de las aristas seleccionadas quedan desunidos. A diferencia de la anulación del pliegue, que crea una transición brusca alrededor de la arista manteniendo la conexión, la anulación de la soldadura crea una discontinuidad. Esto puede comprobarse mediante la comparación del número de vértices antes y después de realizar la operación. Cualquier operación posterior en las aristas o los vértices no soldados también demostrará que la superficie está desconectada a lo largo de la arista no soldada.

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
