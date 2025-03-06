## Em profundidade
IsNull retornará um valor booleano baseado em se um objeto é nulo. No exemplo abaixo, uma grade de círculos é desenhada com raios variáveis com base no nível Vermelho em um bitmap. Onde não houver um valor Vermelho, nenhum círculo será desenhado e nulo será retornado na lista de círculos. Passar essa lista por IsNull retornará uma lista de valores booleanos, com true representando cada localização de um valor nulo. Essa lista de booleanos pode ser usada com List.FilterByBoolMask para retornar uma lista sem nulos.
___
## Arquivo de exemplo

![IsNull](./DSCore.Object.IsNull_img.jpg)

