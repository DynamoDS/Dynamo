## Em profundidade
`List.AllTrue` retornará False se qualquer item na lista fornecida for False ou não for um valor booleano. `List.AllTrue` retornará True se cada item na lista for um valor booleano e True.

No exemplo abaixo, usamos `List.AllTrue` para avaliar listas de valores booleanos. A primeira lista tem um valor False, então False é retornado. A segunda lista tem somente valores True, então True é retornado. A terceira lista tem uma sublista que inclui um valor False, então False é retornado. O nó final avalia as duas sublistas e retorna False para a primeira porque ela tem um valor False e True para a segunda porque ela só tem valores True.
___
## Arquivo de exemplo

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
