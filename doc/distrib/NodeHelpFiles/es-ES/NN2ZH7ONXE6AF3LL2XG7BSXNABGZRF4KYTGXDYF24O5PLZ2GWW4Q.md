<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## En detalle:
Los modos de cuadro y suavizado son dos formas de ver una superficie de T-Spline. El modo de suavizado muestra la forma real de una superficie de T-Spline y es útil para previsualizar la estética y las dimensiones del modelo. Por su parte, el modo de cuadro puede arrojar luz sobre la estructura de la superficie y ofrecer una mejor comprensión de esta, además de ser una opción más rápida para previsualizar geometrías grandes o complejas. El nodo `TSplineSurface.EnableSmoothMode` permite alternar entre estos dos estados de previsualización en distintas fases del desarrollo de la geometría.

En el ejemplo siguiente, la operación de biselado se realiza en una superficie de cuadro de T-Spline. El resultado se visualiza primero en modo de cuadro (la entrada `inSmoothMode` de la superficie de cuadro ajustada a "false", falso) para comprender mejor la estructura de la forma. A continuación, se activa el modo de suavizado mediante el nodo `TSplineSurface.EnableSmoothMode` y el resultado se traslada a la derecha para previsualizar ambos modos al mismo tiempo.
___
## Archivo de ejemplo

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
