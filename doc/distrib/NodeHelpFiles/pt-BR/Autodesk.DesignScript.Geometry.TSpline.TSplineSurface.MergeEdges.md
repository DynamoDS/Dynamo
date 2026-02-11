## Em profundidade
No exemplo abaixo, é criada uma superfície da T-Spline por extrusão de uma curva NURBS. Seis de suas arestas são selecionadas com um nó `TSplineTopology.EdgeByIndex` – três em cada lado da forma. Os dois conjuntos de arestas, juntamente com a superfície, são passados para o nó `TSplineSurface.MergeEdges`. A ordem dos grupos de aresta afeta a forma – o primeiro grupo de arestas é deslocado para atender ao segundo grupo, que permanece no mesmo lugar. A entrada `insertCreases’ adiciona a opção de dobra da junção ao longo das arestas mescladas. O resultado da operação de mesclagem é convertido na lateral para oferecer uma melhor visualização.
___
## Arquivo de exemplo

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
