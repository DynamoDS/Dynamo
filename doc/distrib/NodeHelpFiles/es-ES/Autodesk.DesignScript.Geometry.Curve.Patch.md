## En detalle:
Patch intentará crear una superficie mediante una curva de entrada como contorno. La curva de entrada debe estar cerrada. En el siguiente ejemplo, se utiliza primero un nodo Point.ByCylindricalCoordinates para crear un conjunto de puntos en intervalos establecidos en un círculo, pero con elevaciones y radios aleatorios. A continuación, se utiliza un nodo NurbsCurve.ByPoints para crear una curva cerrada basada en estos puntos. Se usa un nodo Patch para crear una superficie desde la curva cerrada del contorno. Tenga en cuenta que, como los puntos se han creado con radios y elevaciones aleatorios, no todas las disposiciones dan como resultado una curva a la que se le puede aplicar un parche.
___
## Archivo de ejemplo

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

