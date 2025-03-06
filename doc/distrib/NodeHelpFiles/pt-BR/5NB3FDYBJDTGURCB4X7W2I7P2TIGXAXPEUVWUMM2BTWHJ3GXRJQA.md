<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## Em profundidade
`Curve.Extrude (curve, direction, distance)` efetua a extrusão de uma curva de entrada usando um vetor de entrada para determinar a direção da extrusão. Uma entrada `distance` separada é usada para a distância de extrusão.

No exemplo abaixo, primeiro criamos uma NurbsCurve usando um nó `NurbsCurve.ByControlPoints`, com um conjunto de pontos gerados aleatoriamente como entrada. Um bloco de código é usado para especificar os componentes X, Y e Z de um nó `Vector.ByCoordinates`. Esse vetor é usado como entrada de direção em um nó `Curve.Extrude`, enquanto um `number slider` é usado para controlar a entrada `distance`.
___
## Arquivo de exemplo

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
