<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## En detalle:
`Curve.ExtrudeAsSolid (curve, distance)` extruye una curva plana cerrada de entrada mediante un número de entrada para determinar la distancia de la extrusión. La dirección de la extrusión viene determinada por el vector normal del plano en el que se encuentra la curva. Este nodo tapa los extremos de la extrusión para crear un sólido.

En el ejemplo siguiente, creamos primero una NurbsCurve mediante el nodo `NurbsCurve.ByPoints` con un conjunto de puntos generados aleatoriamente como entrada. A continuación, se utiliza el nodo `Curve.ExtrudeAsSolid` para extruir la curva como un sólido. Se utiliza un control deslizante de número como entrada `distance` en el nodo `Curve.ExtrudeAsSolid`.
___
## Archivo de ejemplo

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
