## In profondità
`Mesh.Nearest` restituisce un punto sulla mesh di input più vicino al punto specificato. Il punto restituito è una proiezione del punto di input sulla mesh utilizzando il vettore normale alla mesh passante per il punto risultante nel punto più vicino.

Nell'esempio seguente, viene creato un semplice caso d'uso per mostrare come funziona il nodo. Il punto di input si trova al di sopra di una mesh sferica, ma non direttamente in cima. Il punto risultante è il punto più vicino a cui si trova la mesh. Ciò è in contrasto con l'output del nodo `Mesh.Project` (utilizzando lo stesso punto e la stessa mesh degli input insieme ad un vettore nella direzione 'Z' negativa) dove il punto risultante viene proiettato sulla mesh direttamente sotto il punto di input. `Line.ByStartAndEndPoint` viene utilizzato per mostrare la 'traiettoria' del punto proiettato sulla mesh.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.Nearest_img.jpg)
