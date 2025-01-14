## Em profundidade
`Curve.Extrude (curve, distance)` efetua a extrusão de uma curva de entrada usando um número de entrada para determinar a distância da extrusão. A direção do vetor normal ao longo da curva é usada para a direção de extrusão.

No exemplo abaixo, primeiro criamos uma NurbsCurve usando um nó `NurbsCurve.ByControlPoints`, com um conjunto de pontos gerados aleatoriamente como entrada. Em seguida, usamos um nó `Curve.Extrude` para efetuar a extrusão da curva. Um controle deslizante numérico é usado como entrada `distance` no nó `Curve.Extrude`.
___
## Arquivo de exemplo

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
