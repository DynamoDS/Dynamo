## Em profundidade
Curve by Parameter Line On Surface criará uma linha ao longo de uma superfície entre duas coordenadas UV de entrada. No exemplo abaixo, primeiro criamos uma grade de pontos e os convertemos na direção Z por um valor aleatório. Esses pontos são usados para criar a superfície usando um nó NurbsSurface.ByPoints. Essa superfície é usada como baseSurface de um nó ByParameterLineOnSurface. Um conjunto de controles deslizantes de números são usados para ajustar as entradas U e V de dois nós UV.ByCoordinates, que são usados para determinar o ponto inicial e final da linha na superfície.
___
## Arquivo de exemplo

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

