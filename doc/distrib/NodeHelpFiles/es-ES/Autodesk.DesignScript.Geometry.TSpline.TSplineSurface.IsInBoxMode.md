## En detalle:
Los modos de cuadro y suavizado son dos formas de ver una superficie de T-Spline. El modo de suavizado es la forma real de una superficie de T-Spline y resulta útil para previsualizar la estética y las dimensiones del modelo. Por su parte, el modo de cuadro puede dar una idea de la estructura de la superficie y permite comprenderla mejor, además de ser una opción más rápida para previsualizar geometrías grandes o complejas. Estos dos modos pueden controlarse en el momento de crear la superficie de T-Spline inicial o más tarde con nodos como `TSplineSurface.EnableSmoothMode`.

En los casos en los que una T-Spline se vuelve no válida, su vista preliminar cambia automáticamente al modo de cuadro. El uso del nodo `TSplineSurface.IsInBoxMode` es otra forma de identificar si la superficie deja de ser válida.

En el ejemplo siguiente, se crea una superficie de plano de T-Spline con la entrada `smoothMode` establecida en el valor "True" (verdadero). Se suprimen dos de sus caras, lo que invalida la superficie. La vista preliminar de la superficie cambia al modo de cuadro, aunque es imposible saberlo solo con la vista preliminar. El nodo `TSplineSurface.IsInBoxMode` se utiliza para confirmar que la superficie se encuentra en el modo de cuadro.
___
## Archivo de ejemplo

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
