## In profondità
`List.FilterByBoolMask` utilizza due elenchi come input. Il primo elenco viene suddiviso in due elenchi separati in base ad un elenco corrispondente di valori booleani (True o False). Gli elementi dell'input `list` che corrispondono a True nell'input `mask` vengono indirizzati all'output denominato In, mentre gli elementi che corrispondono ad un valore False vengono indirizzati all'output denominato `out`.

Nell'esempio seguente, `List.FilterByBoolMask` viene utilizzato per selezionare il legno e il laminato da un elenco di materiali. Prima vengono confrontati due elenchi per trovare gli elementi corrispondenti, quindi viene utilizzato un operatore `Or` per verificare la presenza di elementi dell'elenco True. Quindi, gli elementi dell'elenco vengono filtrate a seconda che siano legno o laminato o altro.
___
## File di esempio

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
