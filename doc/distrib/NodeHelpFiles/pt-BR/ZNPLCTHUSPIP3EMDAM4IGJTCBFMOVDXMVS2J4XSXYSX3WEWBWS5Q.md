<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## Em profundidade
Coordinate System At Segment Length retornará um sistema de coordenadas alinhado com a curva de entrada no comprimento da curva especificado, medido desde o ponto inicial da curva. O sistema de coordenadas resultante terá seu eixo X na direção da normal da curva e o eixo y na direção da tangente da curva no comprimento especificado. No exemplo abaixo, primeiro criaremos uma curva Nurbs usando um nó ByControlPoints, com um conjunto de pontos gerados automaticamente como entrada. Um controle deslizante de número é usado para controlar a entrada de comprimento de segmento para um nó CoordinateSystemAtParameter. Se o comprimento especificado for maior que o comprimento da curva, esse nó retornará um sistema de coordenadas no ponto final da curva.
___
## Arquivo de exemplo

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

