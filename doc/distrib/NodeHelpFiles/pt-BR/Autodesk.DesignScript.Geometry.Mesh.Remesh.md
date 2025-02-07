## Em profundidade
'Mesh.Remesh' cria uma nova malha na qual os triângulos de um determinado objeto são redistribuídos de forma mais uniforme, independentemente de qualquer alteração nas normais dos triângulos. Essa operação pode ser útil para malhas com densidade variável de triângulos para preparar a malha para análise de resistência. Redefinir uma malha repetidamente gera malhas progressivamente mais uniformes. Para malhas cujos vértices já são equidistantes (por exemplo, uma malha de icosfera), o resultado do nó 'Mesh.Remesh' é a mesma malha.
No exemplo abaixo, 'Mesh.Remesh' é usado em uma malha importada com uma alta densidade de triângulos em áreas com detalhes finos. O resultado do nó 'Mesh.Remesh' é convertido ao lado e 'Mesh.Edges' é usado para visualizar o resultado.

'(O arquivo de exemplo usado está licenciado sob Creative Commons)'

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Remesh_img.jpg)
