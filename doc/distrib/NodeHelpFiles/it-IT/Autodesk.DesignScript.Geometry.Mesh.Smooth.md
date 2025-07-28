## In profondità
Questo nodo restituisce una nuova mesh levigata utilizzando un algoritmo di levigatezza cotangente che non allontana i vertici dalla loro posizione originale ed è migliore per mantenere oggetti e bordi. È necessario immettere un valore di scala nel nodo per impostare la scala spaziale della levigatezza. I valori di scala possono essere compresi tra 0,1 e 64,0. Valori più elevati si traducono in un effetto di levigatezza più evidente, che si traduce in una mesh apparentemente più semplice. Nonostante sembri più levigata e semplice, la nuova mesh ha lo stesso conteggio di triangoli, bordi e vertici di quella iniziale.

Nell'esempio seguente, `Mesh.ImportFile` viene utilizzato per importare un oggetto. `Mesh.Smooth` viene quindi utilizzato per levigare l'oggetto, con una scala di levigatezza di 5. L'oggetto viene quindi traslato in un'altra posizione con `Mesh.Translate` per una migliore anteprima e `Mesh.TriangleCount` viene utilizzato per tenere traccia del numero di triangoli nella vecchia mesh e in quella nuova.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.Smooth_img.jpg)
