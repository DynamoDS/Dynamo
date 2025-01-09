## Em profundidade
Fillet retornará um novo sólido com arestas arredondadas. A entrada de arestas especifica quais arestas serão arredondadas, enquanto a entrada de deslocamento determina o raio da concordância. No exemplo abaixo, começaremos com um cubo usando as entradas padrão. Para obter as arestas apropriadas do cubo, explodimos primeiro o cubo para obter as faces como uma lista de superfícies. Em seguida, usamos um nó Face.Edges para extrair as arestas do cubo. Extraímos a primeira aresta de cada face com GetItemAtIndex. Um controle deslizante de número controla o raio de cada concordância.
___
## Arquivo de exemplo



