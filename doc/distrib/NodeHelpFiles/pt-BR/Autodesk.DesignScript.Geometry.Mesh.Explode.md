## Em profundidade
O nó 'Mesh.Explode' usa uma única malha e retorna uma lista de faces de malha como malhas independentes.

O exemplo abaixo mostra um domo de malha que é explodido usando 'Mesh.Explode', seguido por um deslocamento de cada face na direção da normal da face. Isso é obtido usando os nós 'Mesh.TriangleNormals' e 'Mesh.Translate'. Embora neste exemplo as faces de malha pareçam quadrados, na verdade são triângulos com normais idênticas.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
