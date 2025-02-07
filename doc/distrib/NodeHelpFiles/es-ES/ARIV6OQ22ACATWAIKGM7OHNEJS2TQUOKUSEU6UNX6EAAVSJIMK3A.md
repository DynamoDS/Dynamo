<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## En detalle:
El nodo `TSplineSurface.CompressIndexes` elimina los huecos en los números de índice de aristas, vértices o caras de una superficie de T-Spline que se derivan de varias operaciones como, por ejemplo, Suprimir cara. Se conserva el orden de los índices.

En el ejemplo siguiente, se eliminan varias caras de una superficie de primitiva de esfera de malla cuadrada, lo que afecta a los índices de aristas de la forma. Se utiliza `TSplineSurface.CompressIndexes` para reparar los índices de aristas de la forma y, de este modo, es posible seleccionar una arista con el índice 1.

## Archivo de ejemplo

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
