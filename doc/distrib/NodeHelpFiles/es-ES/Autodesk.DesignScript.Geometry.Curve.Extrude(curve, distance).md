## En detalle:
`Curve.Extrude (curve, distance)` extruye una curva de entrada mediante un número de entrada para determinar la distancia de la extrusión. La dirección del vector normal a lo largo de la curva se utiliza para la dirección de la extrusión.

En el ejemplo siguiente, creamos primero una NurbsCurve mediante un nodo `NurbsCurve.ByControlPoints` con un conjunto de puntos generados aleatoriamente como entrada. A continuación, utilizamos un nodo `Curve.Extrude` para extruir la curva. Se utiliza un control deslizante de número como entrada `distance` en el nodo `Curve.Extrude`.
___
## Archivo de ejemplo

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
