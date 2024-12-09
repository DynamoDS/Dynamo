## Em profundidade
Curve by IsoCurve on Surface criará uma curva que é a isocurva numa superfície, especificando a direção U ou V e especificando o parâmetro na direção oposta na qual criar a curva. A entrada `direction` determina qual direção da isocurva será criada. Um valor de um corresponde à direção U, enquanto um valor de zero corresponde à direção V. No exemplo abaixo, primeiro criamos uma grade de pontos e os convertemos na direção Z por um valor aleatório. Esses pontos são usados para criar a superfície usando um nó NurbsSurface.ByPoints. Essa superfície é usada como a baseSurface de um nó ByIsoCurveOnSurface. Um controle deslizante de número definido como um intervalo de 0 a 1 e um passo de 1 é usado para controlar se extraímos a isocurva na direção u ou v. Um segundo controle deslizante de número é usado para determinar o parâmetro no qual a isocurve é extraída.
___
## Arquivo de exemplo

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

