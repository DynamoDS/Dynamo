## In profondità
Patch tenterà di creare una superficie utilizzando una curva di input come contorno. La curva di input deve essere chiusa. Nell'esempio seguente, viene prima utilizzato un nodo Point.ByCylindricalCoordinates per creare un insieme di punti ad intervalli impostati in un cerchio, ma con quote altimetriche e raggi casuali. Viene quindi utilizzato un nodo NurbsCurve.ByPoints per creare una curva chiusa basata su questi punti. Viene utilizzato un nodo Patch per creare una superficie dalla curva chiusa del contorno. Notare che, poiché i punti sono stati creati con raggi e quote altimetriche casuali, non tutte le disposizioni producono una curva che può essere coperta.
___
## File di esempio

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

