## Em profundidade
No exemplo abaixo, é criada uma caixa da T-Spline usando o nó `TSplineSurface.ByBoxLengths` com uma origem, uma largura, um comprimento, uma altura, vãos e uma simetria especificados.
`EdgeByIndex` é usado para selecionar uma aresta da lista de arestas da superfície gerada. A aresta selecionada é feita para deslizar ao longo das arestas adjacentes usando `TSplineSurface.SlideEdges`, seguida por seus equivalentes simétricos.
___
## Arquivo de exemplo

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
