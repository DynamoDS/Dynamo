<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## En detalle:
`Curve.NormalAtParameter (curve, param)` devuelve un vector alineado con la dirección normal en el parámetro especificado de una curva. La parametrización de una curva se mide en el rango de 0 a 1, siendo 0 el inicio de la curva y 1 el final.

En el ejemplo siguiente, creamos primero una NurbsCurve mediante el nodo `NurbsCurve.ByControlPoints` con un conjunto de puntos generados aleatoriamente como entrada. Se utiliza un control deslizante de número establecido en el rango de 0 a 1 para controlar la entrada `parameter` del nodo `Curve.NormalAtParameter`.
___
## Archivo de ejemplo

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
