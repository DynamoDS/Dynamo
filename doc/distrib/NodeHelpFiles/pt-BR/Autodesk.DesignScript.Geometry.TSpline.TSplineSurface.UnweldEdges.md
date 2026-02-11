## In-Depth

No exemplo abaixo, a operação Desfazer solda é executada na linha de arestas de uma superfície da T-Spline. Como resultado, os vértices das arestas selecionadas são desunidos. Ao contrário de Desfazer dobra, que cria uma transição aguda em torno da aresta enquanto mantém a conexão, Desfazer solda cria uma descontinuidade. Isso pode ser comprovado pela comparação do número de vértices antes e depois da operação ser executada. Todas as operações subsequentes em arestas ou vértices não soldados também demonstrarão que a superfície está desconectada ao longo da aresta não soldada.

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
