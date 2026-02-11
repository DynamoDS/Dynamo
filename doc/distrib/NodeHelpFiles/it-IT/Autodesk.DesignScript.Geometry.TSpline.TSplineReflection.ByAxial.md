## In-Depth
`TSplineReflection.ByAxial` restituisce un oggetto `TSplineReflection` che può essere utilizzato come input per il nodo `TSplineSurface.AddReflections`.
L'input del nodo `TSplineReflection.ByAxial` è un piano che funge da piano speculare. Analogamente a TSplineInitialSymmetry, TSplineReflection, una volta definito per TSplineSurface, influenza tutte le successive operazioni e alterazioni.

Nell'esempio seguente, `TSplineReflection.ByAxial` viene utilizzato per creare un oggetto TSplineReflection posizionato nella parte superiore del cono T-Spline. Il riflesso viene quindi utilizzato come input per i nodi `TSplineSurface.AddReflections` per riflettere il cono e restituire una nuova superficie T-Spline.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
