## In profondità
Nell'esempio seguente, una superficie T-Spline viene creata estrudendo una curva NURBS. Sei dei suoi bordi vengono selezionati con un nodo `TSplineTopology.EdgeByIndex`, tre su ciascun lato della forma. I due gruppi di bordi, insieme alla superficie, vengono passati al nodo `TSplineSurface.MergeEdges`. L'ordine dei gruppi di bordi influisce sulla forma, ovvero il primo gruppo di bordi viene spostato per soddisfare il secondo gruppo, che rimane nella stessa posizione. L'input `insertCreases` aggiunge l'opzione di triangolazione del giunto lungo i bordi uniti. Il risultato dell'operazione di unione viene traslato sul lato per una migliore anteprima.
___
## File di esempio

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
