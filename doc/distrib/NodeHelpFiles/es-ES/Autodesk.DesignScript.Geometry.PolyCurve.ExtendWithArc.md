## En detalle:
ExtendWithArc añadirá un arco circular al principio o al final de una PolyCurve de entrada y devolverá una única PolyCurve combinada. La entrada de radio determinará el radio del círculo, mientras que la entrada de longitud determinará la distancia a lo largo del círculo para el arco. La longitud total debe ser menor o igual que la longitud de un círculo completo con el radio especificado. El arco generado será tangente al extremo de la PolyCurve de entrada. Una entrada booleana para endOrStart controla el extremo de la PolyCurve para la que se creará el arco. El valor "true" (verdadero) creará el arco al final de la PolyCurve, mientras que el valor "false" (falso) creará el arco al principio de la PolyCurve. En el siguiente ejemplo, se usa primero un conjunto de puntos aleatorios y PolyCurve.ByPoints para generar una PolyCurve. A continuación, se usan dos controles deslizantes de número y un conmutador booleano para establecer los parámetros de ExtendWithArc.
___
## Archivo de ejemplo

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

