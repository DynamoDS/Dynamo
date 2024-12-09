## Em profundidade
Retorna uma nova malha com os seguintes defeitos corrigidos:
- Componentes pequenos: se a malha contiver segmentos desconectados muito pequenos (em relação ao tamanho geral da malha), eles serão descartados.
- Furos: os furos na malha são preenchidos.
- Regiões não múltiplas: se um vértice estiver conectado a mais de duas arestas de *limite* ou uma aresta estiver conectada a mais de dois triângulos, o vértice ou a aresta não serão múltiplos. O kit de ferramentas de malha removerá a geometria até que a malha seja múltipla.
Esse método tenta preservar o máximo possível da malha original, ao contrário de MakeWatertight, que faz nova amostragem da malha.

No exemplo abaixo, 'Mesh.Repair' é usado em uma malha importada para preencher o furo ao redor da orelha do coelho.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Repair_img.jpg)
