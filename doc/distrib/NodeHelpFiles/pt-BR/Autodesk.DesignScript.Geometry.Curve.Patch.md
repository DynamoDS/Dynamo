## Em profundidade
Patch tentará criar uma superfície usando uma curva de entrada como limite. A curva de entrada deve ser fechada. No exemplo abaixo, primeiro usaremos um nó Point.ByCylindricalCoordinates para criar um conjunto de pontos em intervalos definidos em um círculo, mas com elevações e raios aleatórios. Em seguida, usaremos um nó NurbsCurve.ByPoints para criar uma curva fechada com base nesses pontos. Um nó Patch é usado para criar uma superfície com base na curva fechada de limite. Observe que como os pontos foram criados com raios e elevações aleatórios, nem todas as disposições resultam em uma curva que pode ser corrigida.
___
## Arquivo de exemplo

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

