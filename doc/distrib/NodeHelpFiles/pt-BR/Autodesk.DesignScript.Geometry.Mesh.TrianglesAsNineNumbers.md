## Em profundidade
'Mesh.TrainglesAsNineNumbers' determina as coordenadas X, Y e Z dos vértices que compõem cada triângulo em uma malha fornecida, resultando em nove números por triângulo. Esse nó pode ser útil para consultar, reconstruir ou converter a malha original.

No exemplo abaixo, 'File Path' e 'Mesh.ImportFile' são usados para importar uma malha. Em seguida, 'Mesh.TrianglesAsNineNumbers' é usado para obter as coordenadas dos vértices de cada triângulo. Essa lista é subdividida em três usando 'List.Chop' com a entrada 'lengths' definida como 3. 'List.GetItemAtIndex' é então usado para obter cada coordenada X, Y e Z e reconstruir os vértices usando 'Point.ByCoordinates'. A lista de pontos é dividida em três (3 pontos para cada triângulo) e é usada como entrada para 'Polygon.ByPoints'.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
