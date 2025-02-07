## Em profundidade
Plane At Segment Length retornará um plano alinhado com uma curva em um ponto que esteja a uma distância especificada ao longo da curva, medida desde o ponto inicial. Se o comprimento de entrada for maior que o comprimento total da curva, esse nó usará o ponto final da curva. O vetor normal do plano resultante corresponderá à tangente da curva. No exemplo abaixo, primeiro criaremos uma curva Nurbs usando um nó ByControlPoints, com um conjunto de pontos gerados aleatoriamente como entrada. Um controle deslizante de número é usado para controlar a entrada de parâmetro para um nó PlaneAtSegmentLength.
___
## Arquivo de exemplo

![PlaneAtSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength_img.jpg)

