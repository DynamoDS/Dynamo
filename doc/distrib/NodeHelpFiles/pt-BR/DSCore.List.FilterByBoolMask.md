## Em profundidade
`List.FilterByBoolMask `usa duas listas como entradas. A primeira lista é dividida em duas listas separadas de acordo com uma lista correspondente de valores booleanos (True ou False). Os itens da entrada `list` que correspondem a True na entrada `mask` são direcionados para a saída identificada como `In`, enquanto os itens que correspondem a um valor False são direcionados para a saída identificada como `out`.

No exemplo abaixo, `List.FilterByBoolMask` é usado para selecionar madeira e laminado de uma lista de materiais. Primeiro comparamos duas listas para localizar itens correspondentes, depois usamos um operador `Or` para verificar itens da lista com valor True. Em seguida, os itens da lista são filtrados dependendo se são madeira ou laminado, ou outra coisa.
___
## Arquivo de exemplo

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
