## In-Depth
`TSplineVertex.IsStarPoint` devuelve si un vértice es un punto de estrella.

Los puntos de estrella aparecen cuando se juntan 3, 5 o más aristas. Aparecen de forma natural en las primitivas de cuadro o de esfera de malla cuadrada y se crean con más frecuencia al extruir una cara de T-Spline, eliminar una cara o realizar una fusión. A diferencia de los vértices normales y de puntos T, los puntos de estrella no están gestionados por filas rectangulares de puntos de control. Los puntos de estrella hacen que el área a su alrededor sea más difícil de controlar y pueden crear distorsión, por lo que solo deben utilizarse cuando sea necesario. Entre las ubicaciones inadecuadas para la colocación de puntos de estrella, se incluyen las partes más afiladas del modelo, como los bordes plegados, las partes en las que la curvatura cambia significativamente o en el borde de una superficie abierta.

Los puntos de estrella también determinan cómo se convertirá una T-Spline en una representación de contorno (BREP). Cuando una T-Spline se convierte en BREP, se divide en superficies independientes en cada punto de estrella.

En el ejemplo siguiente, `TSplineVertex.IsStarPoint` se utiliza para consultar si el vértice seleccionado con `TSplineTopology.VertexByIndex` es un punto de estrella.


## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
