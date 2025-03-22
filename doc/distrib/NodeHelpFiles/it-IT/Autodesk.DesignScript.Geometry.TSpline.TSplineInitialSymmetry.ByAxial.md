## In profondità
`TSplineInitialSymmetry.ByAxial`definisce se la geometria T-Spline presenta una simmetria lungo un asse scelto (x, y, z). La simmetria può verificarsi su uno, due o tutti e tre gli assi. Una volta stabilita alla creazione della geometria T-spline, la simmetria influenza tutte le operazioni e le modifiche successive.

Nell'esempio seguente, `TSplineSurface.ByBoxCorners` viene utilizzato per creare una superficie T-Spline. Tra gli input di questo nodo, `TSplineInitialSymmetry.ByAxial` viene utilizzato per definire la simmetria iniziale nella superficie. `TSplineTopology.RegularFaces` e `TSplineSurface.ExtrudeFaces` vengono quindi utilizzati rispettivamente per selezionare ed estrudere una faccia della superficie T-Spline. Viene quindi creata una copia speculare dell'operazione di estrusione attorno agli assi di simmetria definiti con il nodo `TSplineInitialSymmetry.ByAxial`.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
