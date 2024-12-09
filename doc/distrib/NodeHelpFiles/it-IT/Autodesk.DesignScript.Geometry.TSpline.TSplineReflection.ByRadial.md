## In-Depth
`TSplineReflection.ByRadial` restituisce un oggetto `TSplineReflection` che può essere utilizzato come input per il nodo `TSplineSurface.AddReflections`. Il nodo utilizza un piano come input e la normale del piano funge da asse per la rotazione della geometria. Come TSplineInitialSymmetry, TSplineReflection, una volta stabilita la creazione di TSplineSurface, influenza tutte le successive operazioni e alterazioni.

Nell'esempio seguente, `TSplineReflection.ByRadial` viene utilizzato per definire il riflesso di una superficie T-Spline. Gli input `segmentCount` e `segmentAngle` vengono utilizzati per controllare il modo in cui la geometria viene riflessa attorno alla normale di un determinato piano. L'output del nodo viene quindi utilizzato come input per il nodo `TSplineSurface.AddReflections` per creare una nuova superficie T-Spline.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
