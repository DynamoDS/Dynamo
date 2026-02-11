## En detalle
El nodo `Mesh.ExtrudePolyCurve` extruye una policurva proporcionada por una distancia establecida por la entrada `height` y en la dirección vectorial especificada. Las policurvas abiertas se cierran conectando el primer punto con el último. Si la policurva inicial es plana y no se autointerseca, la malla resultante tiene la opción de cerrarse para formar una malla sólida.
En el ejemplo siguiente, se utiliza `Mesh.ExtrudePolyCurve` para crear una malla cerrada basada en una PolyCurve cerrada.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.ExtrudePolyCurve_img.jpg)
