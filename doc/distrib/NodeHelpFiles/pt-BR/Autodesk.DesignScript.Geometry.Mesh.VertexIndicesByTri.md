## Em profundidade
'Mesh.VertexIndicesByTri' retorna uma lista aplainada de índices de vértices correspondentes a cada triângulo de malha. Os índices são ordenados em três itens, e os agrupamentos de índice podem ser facilmente reconstruídos usando o nó 'List.Chop' com a entrada 'lengths' de 3.

No exemplo abaixo, um 'MeshToolkit.Mesh' com 20 triângulos é convertido em um 'Geometry.Mesh'. 'Mesh.VertexIndicesByTri' é usado para obter a lista de índices que é dividida em listas de três itens usando 'List.Chop'. A estrutura da lista é invertida usando 'List.Transpose' para obter três listas de nível superior de 20 índices correspondentes aos pontos A, B e C em cada triângulo de malha. O nó 'IndexGroup.ByIndices' é usado para criar grupos de índice de três índices cada. A lista estruturada de 'IndexGroups' e a lista de vértices é usada como entrada para 'Mesh.ByPointsFaceIndices' para obter uma malha convertida.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
