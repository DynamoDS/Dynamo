## Em profundidade
'Mesh.CloseCracks' fecha fissuras em uma malha removendo os limites internos de um objeto de malha. Os limites internos podem surgir naturalmente como resultado de operações de modelagem de malha. Triângulos poderão ser excluídos nessa operação se arestas degeneradas forem removidas. No exemplo abaixo, 'Mesh.CloseCracks' é usado em uma malha importada. 'Mesh.VertexNormals' é usado para visualizar os vértices sobrepostos. Depois que a malha original é passada por Mesh.CloseCracks, o número de arestas é reduzido, o que também é evidente comparando a contagem de arestas, usando um nó 'Mesh.EdgeCount'.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
