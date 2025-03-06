## In-Depth
Il nodo `TSplineSurface.BevelEdges` esegue l'offset di un bordo selezionato o di un gruppo di bordi in entrambe le direzioni lungo la faccia, sostituendo il bordo originale con una sequenza di bordi che formano un canale.

Nell'esempio seguente, un gruppo di bordi della primitiva di un parallelepipedo T-Spline viene utilizzato come input per il nodo `TSplineSurface.BevelEdges`. L'esempio illustra in che modo i seguenti input influiscono sul risultato:
- `percentage` controlla la distribuzione dei nuovi bordi creati lungo le facce adiacenti, con i valori adiacenti pari a zero che posizionano nuovi bordi più vicini al bordo originale e i valori prossimi a 1 che sono più lontani.
- `numberOfSegments` controlla il numero di nuove facce nel canale.
- `keepOnFace` definisce se i bordi smussati vengono posizionati nel piano della faccia originale. Se il valore è impostato su True, l'input roundness non ha alcun effetto.
- `roundness` controlla l'arrotondamento della smussatura e prevede un valore compreso nell'intervallo tra 0 e 1, con 0 che produce una smussatura diritta e 1 che restituisce una smussatura arrotondata.

La modalità riquadro viene attivata occasionalmente per una migliore comprensione della forma.


## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
