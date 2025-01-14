## In profondità
`List.RemoveIfNot` restituisce un elenco che mantiene gli elementi che corrispondono al tipo di elemento specificato e rimuove tutti gli altri elementi dell'elenco originale.

Per rimuovere elementi, potrebbe essere necessario utilizzare il percorso completo del nodo, ad esempio `Autodesk.DesignScript.Geometry.Surface` nell'input `type`. Per recuperare i percorsi degli elementi dell'elenco, è possibile inserire l'elenco in un nodo `Object.Type`.

Nell'esempio seguente, `List.RemoveIfNot` restituisce un elenco con una riga, rimuovendo gli elementi punto dall'elenco originale perché non corrispondono al tipo specificato.
___
## File di esempio

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
