## In-Depth
`TSplineVertex.IsTPoint` retorna se um vértice é um ponto T. Os pontos T são vértices no final de linhas parciais de pontos de controle.

No exemplo abaixo, `TSplineSurface.SubdivideFaces` é usado em um primitivo de caixa da T-Spline para exemplificar uma das várias formas de adicionar pontos T a uma superfície. O nó `TSplineVertex.IsTPoint` é usado para confirmar que um vértice em um índice é um ponto T. Para visualizar melhor a posição dos pontos T, são usados os nós`TSplineVertex.UVNFrame` e `TSplineUVNFrame.Position`.



## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
