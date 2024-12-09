## En detalle:
`Solid.ByRevolve` crea una superficie girando la curva de perfil especificada alrededor de un eje. El eje se define mediante un punto `axisOrigin` y un vector `axisDirection`. El ángulo inicial determina dónde se inicia la superficie, medido en grados, y `sweepAngle` determina la distancia alrededor del eje para continuar la superficie.

En el ejemplo siguiente, utilizamos una curva generada con una función coseno como curva de perfil y dos controles deslizantes de número para determinar los valores de `startAngle` y `sweepAngle`. En el caso de `axisOrigin` y `axisDirection`, se dejan los valores por defecto del origen y el eje Z universales para este ejemplo.

___
## Archivo de ejemplo

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

