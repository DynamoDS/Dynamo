## Em profundidade
`List.Sublists` retorna uma série de sublistas de uma determinada lista com base no intervalo de entrada e deslocamento. O intervalo determina os elementos da lista de entrada que são colocados na primeira sublista. Um deslocamento é aplicado ao intervalo e o novo intervalo determina a segunda sublista. Esse processo se repete, aumentando o índice inicial do intervalo pelo deslocamento fornecido até que a sublista resultante esteja vazia.

No exemplo abaixo, começamos com um intervalo de números de 0 a 9. O intervalo de 0 a 5 é usado como intervalo da sublista, com um deslocamento de 2. Na saída de sublistas aninhadas, a primeira lista contém os elementos com índices no intervalo 0...5, e a segunda lista contém os elementos com índices 2...7. À medida que isso é repetido, as sublistas subsequentes tornam-se mais curtas à medida que o final do intervalo excede o comprimento da lista inicial.
___
## Arquivo de exemplo

![List.Sublists](./DSCore.List.Sublists_img.jpg)
