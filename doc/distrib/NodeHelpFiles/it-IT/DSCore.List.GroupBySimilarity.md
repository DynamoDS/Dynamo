## In profondità
I cluster `List.GroupBySimilarity` elencano gli elementi in base all'adiacenza dei relativi indici e alla similitudine dei relativi valori. L'elenco degli elementi da raggruppare può contenere numeri (numeri interi e a virgola mobile) o stringhe, ma non una combinazione di entrambi.

Utilizzare l'input `tolerance` per determinare la similitudine degli elementi. Per gli elenchi di numeri, il valore `tolerance` rappresenta la differenza massima consentita tra due numeri affinché siano considerati simili.

Per gli elenchi di stringhe, `tolerance` rappresenta il numero massimo di caratteri che possono differire tra due stringhe, utilizzando la distanza di Levenshtein per il confronto. La tolleranza massima per le stringhe è impostata su 10.

L'input booleano `considerAdjacency` indica se l'adiacenza deve essere presa in considerazione quando si raggruppano gli elementi. Se è True, solo gli elementi adiacenti simili verranno raggruppati insieme. Se è False, verrà utilizzata solo la similitudine per formare cluster, indipendentemente dall'adiacenza.

Il nodo genera un elenco di elenchi di valori raggruppati in base all'adiacenza e alla similitudine, nonché un elenco di elenchi degli indici degli elementi raggruppati nell'elenco originale.

Nell'esempio seguente, `List.GroupBySimilarity` viene utilizzato in due modi: per raggruppare un elenco di stringhe solo in base alla similitudine e per raggruppare un elenco di numeri in base all'adiacenza e alla similitudine.
___
## File di esempio

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
