## In profondità
Nell'esempio seguente, tutti i vertici interni della superficie di un piano T-Spline vengono raccolti utilizzando il nodo `TSplineTopology.InnerVertices`. I vertici, insieme alla superficie a cui appartengono, vengono utilizzati come input per il nodo `TSplineSurface.PullVertices`. L'input `geometry' è una sfera situata sopra la superficie del piano. L'input `surfacePoints` è impostato su False e i punti di controllo vengono utilizzati per eseguire l'operazione di estrazione.
___
## File di esempio

![TSplineSurface.PullVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.PullVertices_img.jpg)
