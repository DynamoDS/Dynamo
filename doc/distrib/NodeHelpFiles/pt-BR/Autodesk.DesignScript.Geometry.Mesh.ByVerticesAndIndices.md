## Em profundidade
'Mesh.ByVerticesIndices' usa uma lista de “Pontos”, representando os “vértices” dos triângulos de malha, e uma lista de “índices”, representando como a malha é costurada, e cria uma nova malha. A entrada 'vertices' deve ser uma lista plana de vértices exclusivos na malha. A entrada 'indices' deve ser uma lista plana de inteiros. Cada conjunto de três inteiros designa um triângulo na malha. Os números inteiros especificam o índice do vértice na lista de vértices. A entrada dos índices deve ser indexada em 0, e o primeiro ponto da lista de vértices tem como índice 0.

No exemplo abaixo, um nó 'Mesh.ByVerticesIndices' é usado para criar uma malha usando uma lista de nove 'vértices' e uma lista de 36 'índices', especificando a combinação de vértices para cada um dos 12 triângulos da malha.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)
