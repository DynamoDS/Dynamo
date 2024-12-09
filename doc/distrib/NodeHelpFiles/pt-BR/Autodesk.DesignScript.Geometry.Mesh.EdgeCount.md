## Em profundidade
Esse nó conta o número de arestas em uma malha fornecida. Se a malha for feita de triângulos, que é o caso de todas as malhas em 'MeshToolkit', o nó 'Mesh.EdgeCount' retornará apenas arestas exclusivas. Como resultado, é esperado que o número de arestas não seja o triplo do número de triângulos na malha. Essa suposição pode ser usada para confirmar que a malha não contém faces não soldadas (pode ocorrer em malhas importadas).

No exemplo abaixo, 'Mesh.Cone' e 'Number.Slider' são usados para criar um cone, que é usado como entrada para contar as arestas. Tanto 'Mesh.Edges' quanto 'Mesh.Triangles' podem ser usados para visualizar a estrutura e a grade de uma malha na visualização. 'Mesh.Edges' mostra melhor desempenho para malhas complexas e pesadas.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
