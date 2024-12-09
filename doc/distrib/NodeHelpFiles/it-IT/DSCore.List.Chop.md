## In profondità
`List.Chop` divide un determinato elenco in elenchi più piccoli in base ad un elenco di lunghezze di numeri interi di input. Il primo elenco nidificato contiene il numero di elementi specificato dal primo numero nell'input `lengths`. Il secondo elenco nidificato contiene il numero di elementi specificato dal secondo numero nell'input Lengths e così via. `List.Chop` ripete l'ultimo numero nell'input `lengths` fino a quando tutti gli elementi dell'elenco di input non vengono suddivisi.

Nell'esempio seguente, viene utilizzato un blocco di codice per generare un intervallo di numeri compreso tra 0 e 5, con incrementi di 1. Questo elenco contiene 6 elementi. Si utilizza un secondo blocco di codice per creare un elenco di lunghezze in cui suddividere il primo elenco. Il primo numero di questo elenco è 1, che viene utilizzato da `List.Chop` per creare un elenco nidificato con 1 elemento. Il secondo numero è 3, che crea un elenco nidificato con 3 elementi. Poiché vengono specificate più lunghezze, `List.Chop` include tutti gli elementi rimanenti nel terzo e ultimo elenco nidificato.
___
## File di esempio

![List.Chop](./DSCore.List.Chop_img.jpg)
