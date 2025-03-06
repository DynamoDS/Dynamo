<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## Em profundidade
`Curve.ExtrudeAsSolid (direction, distance)` efetua a extrusão de uma curva plana fechada de entrada usando um vetor de entrada para determinar a direção da extrusão. Uma entrada `distance` separada é usada para a distância de extrusão. Esse nó limita as extremidades da extrusão para criar um sólido.

No exemplo abaixo, primeiro criamos uma NurbsCurve usando um nó `NurbsCurve.ByPoints`, com um conjunto de pontos gerados aleatoriamente como entrada. Um `code block` é usado para especificar os componentes X, Y e Z de um nó `Vector.ByCoordinates`. Esse vetor é usado como entrada de direção em um nó `Curve.ExtrudeAsSolid` enquanto um controle deslizante numérico é usado para controlar a entrada `distance`.
___
## Arquivo de exemplo

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
