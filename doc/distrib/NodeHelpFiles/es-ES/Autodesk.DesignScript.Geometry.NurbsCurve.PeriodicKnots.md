## En detalle:
Utilice `NurbsCurve.PeriodicKnots` cuando necesite exportar una curva NURBS cerrada a otro sistema (por ejemplo, Alias) o cuando ese sistema requiera la curva en su forma periódica. Muchas herramientas CAD requieren esta forma para garantizar la precisión en ambos sentidos.

`PeriodicKnots` devuelve el vector de nudos en forma *periódica* (sin bloquear). `Knots` lo devuelve en forma *bloqueada*. Ambas matrices tienen la misma longitud; son dos formas diferentes de describir la misma curva. En la forma bloqueada, los nudos se repiten al principio y al final, por lo que la curva queda bloqueada en el rango de parámetros. En la forma periódica, el espaciado entre nudos se repite al principio y al final, lo que da como resultado un bucle cerrado perfecto.

En el ejemplo siguiente, se crea una curva NURBS periódica con `NurbsCurve.ByControlPointsWeightsKnots`. Los nodos Watch comparan `Knots` y `PeriodicKnots`, por lo que puede ver la misma longitud, pero con valores diferentes. `Knots` es la forma bloqueada (nudos repetidos en los extremos) y `PeriodicKnots` es la forma sin bloquear con el patrón de diferencia repetido que define la periodicidad de la curva.
___
## Archivo de ejemplo

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
