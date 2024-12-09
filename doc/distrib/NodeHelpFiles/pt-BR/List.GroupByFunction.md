## Em profundidade
`List.GroupByFunction` retorna uma nova lista agrupada por uma função.

A entrada `groupFunction` requer um nó em um estado de função (ou seja, ela retorna uma função). Isso significa que pelo menos uma das entradas do nó não está conectada. Em seguida, o Dynamo executa a função de nó em cada item na lista de entrada de `List.GroupByFunction` para usar a saída como um mecanismo de agrupamento.

No exemplo abaixo, duas listas diferentes são agrupadas usando `List.GetItemAtIndex` como função. Essa função cria grupos (uma nova lista) de cada índice de nível superior.
___
## Arquivo de exemplo

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
