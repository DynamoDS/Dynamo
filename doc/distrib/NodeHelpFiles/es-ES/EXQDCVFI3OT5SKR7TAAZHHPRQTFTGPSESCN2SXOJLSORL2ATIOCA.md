<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## En detalle:
`Curve.ExtrudeAsSolid (direction, distance)` extruye una curva plana cerrada de entrada mediante un vector de entrada para determinar la dirección de la extrusión. Se utiliza una entrada `distance` independiente para la distancia de extrusión. Este nodo tapa los extremos de la extrusión para crear un sólido.

En el ejemplo siguiente, creamos primero una NurbsCurve mediante el nodo `NurbsCurve.ByPoints` con un conjunto de puntos generados aleatoriamente como entrada. Se utiliza un bloque de código para especificar los componentes X, Y y Z del nodo `Vector.ByCoordinates`. Este vector se utiliza como la entrada `direction` del nodo `Curve.ExtrudeAsSolid`, mientras que el control deslizante de número se utiliza para determinar la entrada `distance`.
___
## Archivo de ejemplo

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
