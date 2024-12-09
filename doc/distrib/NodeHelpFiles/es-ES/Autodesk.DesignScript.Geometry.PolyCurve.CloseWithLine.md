## En detalle:
CloseWithLine añade una línea recta entre los puntos inicial y final de una PolyCurve abierta. Devuelve una PolyCurve nueva que incluye la línea añadida. En el siguiente ejemplo, se genera un conjunto de puntos aleatorios y se utiliza PolyCurve.ByPoints con la entrada connectLastToFirst establecida en "false" (falsa) para crear una PolyCurve abierta. Al introducir esta PolyCurve en CloseWithLine, se crea una nueva PolyCurve cerrada que, en este caso, sería equivalente a utilizar una entrada "true" (verdadera) para la opción connectLastToFirst en PolyCurve.ByPoints.
___
## Archivo de ejemplo

![CloseWithLine](./Autodesk.DesignScript.Geometry.PolyCurve.CloseWithLine_img.jpg)

