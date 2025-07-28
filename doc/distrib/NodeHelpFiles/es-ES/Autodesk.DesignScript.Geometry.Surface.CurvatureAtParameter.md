## En detalle:
CurvatureAtParameter utiliza parámetros de entrada U y V y devuelve un sistema de coordenadas basado en la normal, y las direcciones U y V en la posición UV de la superficie. El vector normal determina el eje Z, mientras que las direcciones U y V determinan la dirección de los ejes X e Y. La longitud de los ejes viene determinada por la curvatura U y V. En el siguiente ejemplo, se crea primero una superficie mediante BySweep2Rails. A continuación, se usan dos controles deslizantes de número para determinar los parámetros U y V a fin de crear un sistema de coordenadas con un nodo CurvatureAtParameter.
___
## Archivo de ejemplo

![CurvatureAtParameter](./Autodesk.DesignScript.Geometry.Surface.CurvatureAtParameter_img.jpg)

