<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## Em profundidade
`Curve.ExtrudeAsSolid (curve, distance)` efetua a extrusão de uma curva plana fechada de entrada usando um número de entrada para determinar a distância da extrusão. A direção da extrusão é determinada pelo vetor normal do plano em que a curva se encontra. Esse nó limita as extremidades da extrusão para criar um sólido.

No exemplo abaixo, primeiro criamos uma NurbsCurve usando um nó `NurbsCurve.ByPoints`, com um conjunto de pontos gerados aleatoriamente como entrada. Em seguida, um nó `Curve.ExtrudeAsSolid` é usado para extrusão da curva como um sólido. Um controle deslizante numérico é usado como entrada `distance` no nó `Curve.ExtrudeAsSolid`.
___
## Arquivo de exemplo

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
