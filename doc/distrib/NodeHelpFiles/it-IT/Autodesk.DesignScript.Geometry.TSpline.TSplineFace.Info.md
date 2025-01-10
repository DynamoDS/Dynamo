## In-Depth
`TSplineFace.Info` restituisce le seguenti proprietà di una faccia T-Spline:
- `uvnFrame`: punto sullo scafo, vettore U, vettore V e vettore normale della faccia T-Spline
- `index`: indice della faccia
- `valence`: numero di vertici o bordi che formano una faccia
- `sides`: numero di bordi di ogni faccia T-Spline

Nell'esempio seguente, `TSplineSurface.ByBoxCorners` e `TSplineTopology.RegularFaces` vengono utilizzati per creare rispettivamente una T-Spline e selezionarne le facce. `List.GetItemAtIndex` viene utilizzato per selezionare una faccia specifica della T-Spline e `TSplineFace.Info` viene utilizzato per trovarne le proprietà.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
