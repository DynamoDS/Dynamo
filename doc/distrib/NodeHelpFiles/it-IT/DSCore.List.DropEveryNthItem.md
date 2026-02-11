## In profondità
`List.DropEveryNthItem` rimuove gli elementi dall'elenco di input a intervalli del valore n di input. Il punto iniziale dell'intervallo può essere modificato con l'input `offset`. Ad esempio, se si inserisce 3 in n e si lascia l'offset come valore di default di 0, verranno rimossi gli elementi con indici 2, 5, 8 e così via. Con un offset pari a 1, gli elementi con indici 0, 3, 6 e così via vengono rimossi. Tenere presente che l'offset include l'intero elenco. Per mantenere gli elementi selezionati anziché rimuoverli, vedere `List.TakeEveryNthItem`.

Nell'esempio seguente, prima viene generato un elenco di numeri utilizzando `Range`, quindi viene rimosso ogni altro numero utilizzando 2 come input per `n`.
___
## File di esempio

![List.DropEveryNthItem](./DSCore.List.DropEveryNthItem_img.jpg)
