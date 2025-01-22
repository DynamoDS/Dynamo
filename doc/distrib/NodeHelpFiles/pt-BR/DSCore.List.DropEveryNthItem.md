## Em profundidade
`List.DropEveryNthItem` remove os itens da lista de entrada em intervalos do valor de entrada n. O ponto inicial do intervalo pode ser alterado com a entrada `offset`. Por exemplo, inserir 3 em n e deixar o deslocamento como padrão de 0 removerá os itens com índices 2, 5, 8 etc. Com um deslocamento de 1, os itens com índices 0, 3, 6 etc. serão removidos. Observe que o deslocamento “empacota” em toda a lista. Para manter os itens selecionados em vez de removê-los, consulte `List.TakeEveryNthItem`.

No exemplo abaixo, primeiro geramos uma lista de números usando `Range` e, em seguida, removemos os outros números usando 2 como entrada para `n`.
___
## Arquivo de exemplo

![List.DropEveryNthItem](./DSCore.List.DropEveryNthItem_img.jpg)
