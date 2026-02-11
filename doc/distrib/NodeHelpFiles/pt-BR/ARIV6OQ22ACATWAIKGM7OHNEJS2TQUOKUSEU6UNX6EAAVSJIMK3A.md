<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## Em profundidade
O nó `TSplineSurface.CompressIndexes` remove intervalos em números de índice de arestas, vértices ou faces de uma superfície da T-Spline que resultam de várias operações, como Excluir face. A ordem dos índices é preservada.

No exemplo abaixo, um número de faces é excluído de uma superfície de primitivo quadball que afeta os índices de aresta da forma. `TSplineSurface.CompressIndexes` é usado para reparar os índices de aresta da forma e, portanto, selecionar uma aresta com o índice 1 se torna possível.

## Arquivo de exemplo

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
