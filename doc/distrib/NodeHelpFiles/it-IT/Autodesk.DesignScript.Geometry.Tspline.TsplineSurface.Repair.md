## In profondità
Nell'esempio seguente, una superficie T-Spline diventa non valida, come si può osservare notando le facce sovrapposte nell'anteprima dello sfondo. Il fatto che la superficie non sia valida può essere confermato dall'errore nell'attivazione della modalità uniforme utilizzando il nodo `TSplineSurface.EnableSmoothMode`. Un altro indizio è il nodo `TSplineSurface.IsInBoxMode` che restituisce `true`, anche se la superficie ha inizialmente attivato la modalità uniforme.

Per riparare la superficie, viene passata attraverso un nodo `TSplineSurface.Repair`. Il risultato è una superficie valida, che può essere confermata attivando correttamente la modalità di anteprima uniforme.
___
## File di esempio

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
