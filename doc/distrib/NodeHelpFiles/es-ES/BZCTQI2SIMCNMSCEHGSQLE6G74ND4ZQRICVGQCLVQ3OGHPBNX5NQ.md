<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## En detalle:
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` divide primero una curva en puntos determinados por una lista de entrada de parámetros. A continuación, devuelve los segmentos impares o pares según determine el valor booleano de la entrada `discardEvenSegments`.

En el ejemplo siguiente, creamos primero una NurbsCurve mediante el nodo `NurbsCurve.ByControlPoints` con un conjunto de puntos generados aleatoriamente como entrada. Se utiliza un bloque de código para crear un rango de números entre 0 y 1, escalonado en 0.1. El uso de este como parámetros de entrada para un nodo `Curve.TrimSegmentsByParameter` genera una lista de curvas que son una versión de línea de trazos de la curva original.
___
## Archivo de ejemplo

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
