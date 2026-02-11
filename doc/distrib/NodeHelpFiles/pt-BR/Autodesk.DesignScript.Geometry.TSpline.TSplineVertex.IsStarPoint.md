## In-Depth
`TSplineVertex.IsStarPoint` retorna se um vértice é um ponto de estrela.

Os pontos de estrela existem quando 3, 5 ou mais arestas se juntam. Eles ocorrem naturalmente no primitivo de caixa ou de quadball e são criados com mais frequência ao efetuar a extrusão de uma face da T-Spline, excluir uma face ou executar a Mesclagem. Ao contrário dos vértices normais e ponto T, os pontos de estrela não são controlados por linhas retangulares de pontos de controle. Os pontos de estrela tornam a área em torno deles mais difícil de controlar e podem criar distorção; portanto, eles somente devem ser usados onde necessário. As localizações insatisfatórias para a inserção de pontos de estrela incluem as peças mais agudas do modelo, como arestas dobradas, peças em que a curvatura muda significativamente ou na aresta de uma superfície aberta.

Os pontos de estrela também determinam como uma T-Spline será convertida em representação de limite (BREP). Quando uma T-Spline é convertida em BREP, ela será dividida em superfícies separadas em cada ponto de estrela.

No exemplo abaixo, `TSplineVertex.IsStarPoint` é usado para consultar se o vértice selecionado com `TSplineTopology.VertexByIndex` é um ponto de estrela.


## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
