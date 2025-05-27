## Em profundidade
`Mesh.ByPointsIndices` usa uma lista de “pontos”, representando os “vértices” dos triângulos de malha, e uma lista de “índices”, representando como a malha é unida, e cria uma nova malha. A entrada `points` deve ser uma lista plana de vértices exclusivos na malha. A entrada `indices` deve ser uma lista plana de números inteiros. Cada conjunto de três números inteiros designa um triângulo na malha. Os números inteiros especificam o índice do vértice na lista de vértices. A entrada de índices deve ser indexada em 0, com o primeiro ponto da lista de vértices tendo o índice 0.

No exemplo abaixo, um nó `Mesh.ByPointsIndices` é usado para criar uma malha usando uma lista de nove“pontos” e uma lista de 36 “índices”, especificando a combinação de vértices para cada um dos 12 triângulos da malha.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
