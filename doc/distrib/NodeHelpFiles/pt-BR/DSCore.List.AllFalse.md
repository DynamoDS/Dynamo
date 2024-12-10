## Em profundidade
`List.AllFalse` retornará False se qualquer item na lista fornecida for True ou não for um valor booleano. `List.AllFalse` retornará True se cada item na lista for um valor booleano e False.

No exemplo abaixo, usamos `List.AllFalse` para avaliar listas de valores booleanos. A primeira lista tem um valor True, então False é retornado. A segunda lista tem somente valores False, então True é retornado. A terceira lista tem uma sublista que inclui um valor True, então False é retornado. O nó final avalia as duas sublistas e retorna False para a primeira sublista porque tem um valor True e True para a segunda sublista porque ela só tem valores False.
___
## Arquivo de exemplo

![List.AllFalse](./DSCore.List.AllFalse_img.jpg)
