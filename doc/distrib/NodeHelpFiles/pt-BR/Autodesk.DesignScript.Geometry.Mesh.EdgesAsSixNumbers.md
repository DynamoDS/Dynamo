## Em profundidade
'Mesh.EdgesAsSixNumbers' determina as coordenadas X, Y e Z dos vértices que compõem cada aresta exclusiva em uma malha fornecida, resultando em seis números por aresta. Esse nó pode ser usado para consultar ou reconstruir a malha ou suas arestas.

No exemplo abaixo, 'Mesh.Cuboid' é usado para criar uma malha cuboide, que é usada como entrada para o nó 'Mesh.EdgesAsSixNumbers' para recuperar a lista de arestas expressas como seis números. A lista é subdividida em listas de 6 itens usando 'List.Chop' e, depois, 'List.GetItemAtIndex' e 'Point.ByCoordinates' são usados para reconstruir as listas de pontos iniciais e finais de cada aresta. Por fim, 'List.ByStartPointEndPoint' é usado para reconstruir as arestas da malha.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
