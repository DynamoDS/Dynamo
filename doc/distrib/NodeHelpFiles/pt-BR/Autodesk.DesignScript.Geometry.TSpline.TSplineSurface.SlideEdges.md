## Em profundidade
No exemplo abaixo, é criada uma superfície de caixa da T-Spline simples e uma de suas arestas é selecionada usando o nó `TSplineTopology.EdgeByIndex`. Para obter uma melhor compreensão da posição do vértice escolhido, ela é visualizada com a ajuda dos nós `TSplineEdge.UVNFrame` e `TSplineUVNFrame.Position`. A aresta escolhida é passada como entrada para o nó `TSplineSurface.SlideEdges`, junto com a superfície à qual pertence. A entrada `amount` determina quanto a aresta desliza em direção às arestas vizinhas, expressa como porcentagem. A entrada `roundness` controla a planicidade ou o arredondamento do chanfro. O efeito de arredondamento é mais bem compreendido no modo de caixa. Em seguida, o resultado da operação de deslizamento é convertido na lateral para visualização.

___
## Arquivo de exemplo

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
