## Em profundidade
'GeometryColor.ByMeshColor' retorna um objeto GeometryColor que é uma malha colorida seguindo a lista de cores fornecida. Existem algumas maneiras de usar esse nó:

- se uma cor for fornecida, toda a malha será colorida com uma determinada cor;
- se o número de cores coincidir com o número de triângulos, cada triângulo será colorido com a cor correspondente da lista;
- se o número de cores corresponder ao número de vértices exclusivos, a cor de cada triângulo na cor da malha será interpolada entre os valores de cor em cada vértice;
- se o número de cores for igual ao número de vértices não únicos, a cor de cada triângulo será interpolada entre os valores de cor em uma face, mas não poderá ser mesclada entre as faces.

## Exemplo

No exemplo abaixo, uma malha é codificada por cores com base na elevação de seus vértices. Primeiro, 'Mesh.Vertices' é usado para obter vértices de malha exclusivos que são analisados e a elevação de cada ponto de vértice é obtida usando o nó 'Point.Z'. Em segundo lugar, 'Map.RemapRange' é usado para mapear os valores para um novo intervalo de 0 a 1 dimensionando cada valor proporcionalmente. Por fim, 'Color Range' é usado para gerar uma lista de cores correspondentes aos valores mapeados. Use essa lista de cores como a entrada 'colors' do nó 'GeometryColor.ByMeshColors'. O resultado é uma malha codificada por cores em que a cor de cada triângulo é interpolada entre as cores do vértice, resultando em um gradiente.

## Arquivo de exemplo

![Example](./Modifiers.GeometryColor.ByMeshColors_img.jpg)
