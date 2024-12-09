## Em profundidade
O `List.Chop` divide uma determinada lista em listas menores com base em uma lista de comprimentos de números inteiros de entrada. A primeira lista aninhada contém o número de elementos especificados pelo primeiro número na entrada `lengths`. A segunda lista aninhada contém o número de elementos especificados pelo segundo número na entrada `lengths` e assim por diante. O `List.Chop` repete o último número na entrada `lengths` até que todos os elementos da lista de entrada sejam cortados.

No exemplo abaixo, usamos um bloco de código para gerar um intervalo de números entre 0 e 5, com passo igual a 1. Esta lista tem 6 elementos. Usamos um segundo bloco de código para criar uma lista de comprimentos para cortar a primeira lista. O primeiro número nesta lista é 1, que o `List.Chop` usa para criar uma lista aninhada com 1 item. O segundo número é 3, que cria uma lista aninhada com 3 itens. Como nenhum outro comprimento é especificado, `List.Chop` inclui todos os itens restantes na terceira e última lista aninhada.
___
## Arquivo de exemplo

![List.Chop](./DSCore.List.Chop_img.jpg)
