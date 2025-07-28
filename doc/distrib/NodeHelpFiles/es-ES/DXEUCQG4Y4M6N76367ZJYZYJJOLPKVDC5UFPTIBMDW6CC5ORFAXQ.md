<!--- Autodesk.DesignScript.Geometry.CoordinateSystem.Scale(coordinateSystem, basePoint, from, to) --->
<!--- DXEUCQG4Y4M6N76367ZJYZYJJOLPKVDC5UFPTIBMDW6CC5ORFAXQ --->
## En detalle:
`CoordinateSystem.Scale (coordinateSystem, basePoint, from, to)` devuelve un CoordinateSystem ajustado en función de un punto base de escala, un punto desde el que ajustar la escala y un punto hasta el que ajustar la escala. La entrada `basePoint` define dónde comienza el ajuste de escala (cuánto se desplaza el CoordinateSystem). La distancia entre los puntos de origen `from` y destino `to` define la cantidad de escala.

En el ejemplo siguiente, un `basePoint` de (-1, 2, 0) define desde dónde se debe ajustar la escala. La distancia entre los puntos de origen `from` (1, 1, 0) y de destino `to` (6, 6, 0) determina la cantidad de escala.

___
## Archivo de ejemplo

![CoordinateSystem.Scale](./DXEUCQG4Y4M6N76367ZJYZYJJOLPKVDC5UFPTIBMDW6CC5ORFAXQ_img.jpg)
