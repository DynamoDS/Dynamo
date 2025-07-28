## In-Depth
Esse nó retorna um objeto TSplineUVNFrame que pode ser útil para visualizar a posição e orientação do vértice, bem como para usar os vetores U, V ou N para mais manipulação da superfície da T-Spline.

No exemplo abaixo, o nó `TSplineVertex.UVNFrame` é usado para obter a estrutura UVN do vértice selecionado. A estrutura UVN é usada para retornar a normal do vértice. Por fim, a direção normal é usada para mover o vértice usando o nó `TSplineSurface.MoveVertices`.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
