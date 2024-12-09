## Em profundidade
No exemplo abaixo, as folgas em uma superfície cilíndrica da T-Spline são preenchidas com o nó `TSplineSurface.FillHole`, o qual requer as seguintes entradas:
- `edges`: um número de arestas de borda selecionado na superfície da T-Spline a ser preenchido
- `fillMethod`: um valor numérico de 0-3 que indica o método de preenchimento:
    * 0 preenche o furo com suavização de serrilhado
    * 1 preenche o furo com uma única face de NGon
    * 2 cria um ponto no centro do furo do qual as faces triangulares irradiam em direção às arestas
    * 3 é semelhante ao método 2, com uma diferença de que os vértices centrais são soldados em um vértice em vez de apenas empilhados na parte superior.
- `keepSubdCreases`: um valor booleano que indica se as dobras subdivididas são preservadas.
___
## Arquivo de exemplo

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
