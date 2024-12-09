## In profondità
`List.TakeEveryNthItem` genera un nuovo elenco contenente solo gli elementi dell'elenco di input che si trovano ad intervalli del valore n di input. Il punto iniziale dell'intervallo può essere modificato con l'input `offset`. Ad esempio, se si inserisce 3 in n e si lascia l'offset come valore di default 0, verranno mantenuti gli elementi con indici 2, 5, 8 e così via. Con un offset pari a 1, verranno mantenuti gli elementi con indici 0, 3, 6 e così via. L'offset determina il ritorno a capo dell'intero elenco. Per rimuovere gli elementi selezionati anziché mantenerli, vedere `List.DropEveryNthItem`.

Nell'esempio seguente, viene generato un elenco di numeri utilizzando `Range` e quindi viene mantenuto un numero su due utilizzando 2 come input per n.
___
## File di esempio

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
