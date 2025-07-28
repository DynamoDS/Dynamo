## Em profundidade
`TSplineSurface.FlattenVertices(vertices, parallelPlane)` altera as posições dos pontos de controle para um conjunto especificado de vértices ao alinhá-los com um `parallelPlane` fornecido como entrada.

No exemplo abaixo, os vértices de uma superfície de plano da T-Spline são deslocados usando os nós `TsplineTopology.VertexByIndex` e `TSplineSurface.MoveVertices`. Em seguida, a superfície é convertida na lateral para obter uma melhor visualização e usada como entrada para um nó `TSplineSurface.FlattenVertices(vertices, parallelPlane)`. O resultado é uma nova superfície com vértices selecionados posicionados no plano fornecido.
___
## Arquivo de exemplo

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
