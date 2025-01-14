<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
De forma semelhante a `TSplineSurface.UnweldEdges`, esse nó executa a operação de desfazer solda em um conjunto de vértices. Como resultado, todas as arestas que se unem ao vértice selecionado têm as soldas desfeitas. Ao contrário da operação de desdobramento, que cria uma transição aguda em torno do vértice enquanto mantém a conexão, a opção Desfazer solda cria uma descontinuidade.

No exemplo abaixo, a solda de um dos vértices selecionados de um plano da T-Spline é desfeita com o nó `TSplineSurface.UnweldVertices`. Uma descontinuidade é introduzida ao longo das arestas ao redor do vértice escolhido, que é ilustrado extraindo um vértice para cima com o nó `TSplineSurface.MoveVertices`.

## Arquivo de exemplo

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
