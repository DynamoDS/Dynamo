## In profondità
`List.NormalizeDepth` restituisce un nuovo elenco di profondità uniforme ad un livello specificato, o profondità di elenco.

Come `List.Flatten`, è possibile utilizzare `List.NormalizeDepth` per restituire un elenco unidimensionale (un elenco con un singolo livello). È tuttavia possibile utilizzarlo anche per aggiungere livelli di elenco. Il nodo normalizza l'elenco di input in base alla profondità scelta.

Nell'esempio seguente, un elenco contenente 2 elenchi di profondità diseguale può essere normalizzato in diversi livelli con un dispositivo di scorrimento a numeri interi. Normalizzando le profondità in diversi livelli, l'elenco aumenta o diminuisce in termini di profondità, ma è sempre uniforme. Un elenco di livello 1 restituisce un unico elenco di elementi, mentre un elenco di livello 3 restituisce 2 livelli di sottoelenchi.
___
## File di esempio

![List.NormalizeDepth](./DSCore.List.NormalizeDepth_img.jpg)
