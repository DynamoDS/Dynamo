## En detalle:
Utilice `NurbsCurve.PeriodicControlPoints` cuando necesite exportar una curva NURBS cerrada a otro sistema (por ejemplo, Alias) o cuando ese sistema requiera la curva en su forma periódica. Muchas herramientas CAD requieren esta forma para garantizar la precisión en ambos sentidos.

`PeriodicControlPoints` devuelve los puntos de control en forma *periódica*. `ControlPoints` los devuelve en forma *bloqueada*. Ambas matrices tienen el mismo número de puntos; son dos formas diferentes de describir la misma curva. En la forma periódica, los últimos puntos de control coinciden con los primeros (tantos como el grado de la curva), por lo que la curva se cierra suavemente. La forma bloqueada utiliza un diseño diferente, por lo que las posiciones de los puntos en las dos matrices son diferentes.

En el ejemplo siguiente, se crea una curva NURBS periódica con `NurbsCurve.ByControlPointsWeightsKnots`. Observe cómo los nodos comparan `ControlPoints` y `PeriodicControlPoints`, lo que le permite ver la misma longitud, pero con posiciones de puntos diferentes. Los ControlPoints aparecen en color rojo, por lo que se distinguen claramente de los PeriodicControlPoints, que son de color negro, en la vista previa en segundo plano.
___
## Archivo de ejemplo

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
