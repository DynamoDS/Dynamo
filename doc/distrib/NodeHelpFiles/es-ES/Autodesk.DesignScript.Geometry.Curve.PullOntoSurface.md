## En detalle:
PullOntoSurface creará una curva nueva proyectando una curva de entrada sobre una superficie de entrada mediante los vectores normales de la superficie como las direcciones de proyección. En el siguiente ejemplo, se crea primero una superficie mediante un nodo Surface.BySweep que utiliza curvas generadas según una curva seno. Esta superficie se utiliza como superficie base sobre la que tirar en un nodo PullOntoSurface. Para la curva, se crea un círculo mediante un bloque de código a fin de especificar las coordenadas del punto central y un control deslizante de número para ajustar el radio del círculo. El resultado es una proyección del círculo sobre la superficie.
___
## Archivo de ejemplo

![PullOntoSurface](./Autodesk.DesignScript.Geometry.Curve.PullOntoSurface_img.jpg)

