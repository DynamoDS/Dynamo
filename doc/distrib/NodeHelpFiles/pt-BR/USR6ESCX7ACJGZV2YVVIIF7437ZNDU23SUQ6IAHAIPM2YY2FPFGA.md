## Em profundidade
`TSplineSurface.Thicken(vetor, softEdges)` engrossa uma superfície da T-Spline guiada pelo vetor especificado. A operação de engrossamento duplica a superfície na direção `vector` e, em seguida, conecta as duas superfícies unindo suas arestas. A entrada booleana `softEdges` controla se as arestas resultantes são suavizadas (true) ou dobradas (false).

No exemplo abaixo, uma superfície da T-Spline cilíndrica é engrossada usando o nó `TSplineSurface.Thicken(vector, softEdges)`. A superfície resultante é convertida na lateral para oferecer uma melhor visualização.


___
## Arquivo de exemplo

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
