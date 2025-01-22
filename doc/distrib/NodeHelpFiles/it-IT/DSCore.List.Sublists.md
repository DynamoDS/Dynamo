## In profondità
`List.Sublists` restituisce una serie di sottoelenchi da un determinato elenco in base all'intervallo e all'offset di input. L'intervallo determina gli elementi dell'elenco di input che vengono inseriti nel primo sottoelenco. Viene applicato un offset all'intervallo e il nuovo intervallo determina il secondo sottoelenco. Questo processo viene ripetuto, aumentando l'indice iniziale dell'intervallo in base all'offset specificato fino a quando il sottoelenco risultante non è vuoto.

Nell'esempio seguente, si inizia con un intervallo di numeri compreso tra 0 e 9. L'intervallo da 0 a 5 viene utilizzato come intervallo di sottoelenchi con un offset pari a 2. Nell'output di sottoelenchi nidificati, il primo elenco contiene gli elementi con indici compresi nell'intervallo 0..5 e il secondo elenco contiene gli elementi con indici 2..7. Ripetendo l'operazione, i sottoelenchi successivi diventano più brevi man mano che la fine dell'intervallo supera la lunghezza dell'elenco iniziale.
___
## File di esempio

![List.Sublists](./DSCore.List.Sublists_img.jpg)
