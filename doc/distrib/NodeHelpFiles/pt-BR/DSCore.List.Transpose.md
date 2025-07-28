## Em profundidade
`List.Transpose` troca as linhas e colunas em uma lista de listas. Por exemplo, uma lista que contém 5 sublistas de 10 itens cada seria transposta para 10 listas de 5 itens cada. Valores nulos são inseridos conforme necessário para garantir que cada sublista tenha o mesmo número de itens.

No exemplo, geramos uma lista de números de 0 a 5 e outra lista de letras de A a E. Em seguida, usamos `List.Create` para combiná-los. `List.Transpose` gera 6 listas de 2 itens cada, um número e uma letra por lista. Observe que como uma das listas originais era maior que a outra, `List.Transpose` inseriu um valor nulo para o item não pareado.
___
## Arquivo de exemplo

![List.Transpose](./DSCore.List.Transpose_img.jpg)
