## Em profundidade
`Curve.Extrude (curve, direction)` efetua a extrusão de uma curva de entrada usando um vetor de entrada para determinar a direção da extrusão. O comprimento do vetor é usado para a distância de extrusão.

No exemplo abaixo, primeiro criamos uma NurbsCurve usando um nó `NurbsCurve.ByControlPoints`, com um conjunto de pontos gerados aleatoriamente como entrada. Um bloco de código é usado para especificar os componentes X, Y e Z de um nó `Vector.ByCoordinates`. Esse vetor é usado como entrada `direction` em um nó `Curve.Extrude`.
___
## Arquivo de exemplo

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
