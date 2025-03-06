<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
La valenza funzionale di un vertice va oltre il semplice conteggio dei bordi adiacenti e tiene conto delle linee di griglia virtuali che influiscono sull'unione del vertice nell'area circostante. Fornisce una comprensione più approfondita del modo in cui i vertici e i relativi bordi influenzano la superficie durante le operazioni di deformazione e rifinitura.
Quando utilizzato su vertici regolari e punti a T, il nodo `TSplineVertex.FunctionalValence` restituisce il valore 4, ovvero la superficie è guidata da spline a forma di griglia. Una valenza funzionale diversa da 4 indica che il vertice è un punto a stella e che l'unione attorno al vertice sarà meno uniforme.

Nell'esempio seguente, `TSplineVertex.FunctionalValence` viene utilizzato su due vertici dei punti a T di una superficie del piano T-Spline. Il nodo `TSplineVertex.Valence` restituisce il valore 3, mentre la valenza funzionale dei vertici selezionati è 4, che è specifica per i punti a T. `TSplineVertex.UVNrame` e `TSplineUVNFrame.Position` vengono utilizzati per visualizzare la posizione dei vertici analizzati.

## File di esempio

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
