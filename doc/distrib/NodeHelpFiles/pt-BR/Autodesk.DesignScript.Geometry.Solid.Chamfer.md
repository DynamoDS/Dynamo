## Em profundidade
Chamfer retornará um novo sólido com arestas chanfradas. A entrada de arestas especifica quais arestas serão chanfradas, enquanto a entrada de deslocamento determina a extensão do chanfro. No exemplo abaixo, começaremos com um cubo usando as entradas padrão. Para obter as arestas apropriadas do cubo, explodimos primeiro o cubo para obter as faces como uma lista de superfícies. Em seguida, usaremos um nó Face.Edges para extrair as arestas do cubo. Extraímos a primeira aresta de cada face com GetItemAtIndex. Um controle deslizante de número controla a distância de deslocamento do chanfro.
___
## Arquivo de exemplo

![Chamfer](./Autodesk.DesignScript.Geometry.Solid.Chamfer_img.jpg)

