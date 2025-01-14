## In profondità
`List.AllTrue` restituisce False se qualsiasi elemento nell'elenco specificato è False o non un valore booleano. `List.AllTrue` restituisce True solo se ogni elemento nell'elenco specificato è un valore booleano e True.

Nell'esempio seguente, viene utilizzato `List.AllTrue` per valutare gli elenchi di valori booleani. Il primo elenco ha un valore False, pertanto viene restituito False. Il secondo elenco ha solo valori True, pertanto viene restituito True. Il terzo elenco ha un sottoelenco che include un valore False, pertanto viene restituito False. Il nodo finale valuta i due sottoelenchi e restituisce False per il primo perché ha un valore False e True per il secondo perché ha solo valori True.
___
## File di esempio

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
