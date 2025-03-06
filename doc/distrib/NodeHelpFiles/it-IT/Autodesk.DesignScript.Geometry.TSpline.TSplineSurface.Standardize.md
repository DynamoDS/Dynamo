## In profondità
Il nodo `TSplineSurface.Standardize` viene utilizzato per standardizzare una superficie T-Spline.
Standardizzazione significa preparare una superficie T-Spline per la conversione NURBS e implica l'estensione di tutti i punti a T fino a quando non sono separati da punti a stella mediante almeno due isocurve. La standardizzazione non modifica la forma della superficie, ma può aggiungere punti di controllo per soddisfare i requisiti geometrici necessari per rendere la superficie compatibile con NURBS.

Nell'esempio seguente, una superficie T-Spline generata tramite `TSplineSurface.ByBoxLengths` ha una delle facce suddivise.
Un nodo `TSplineSurface.IsStandard` viene utilizzato per verificare se la superficie è standard, ma produce un risultato negativo.
`TSplineSurface.Standardize` viene quindi utilizzato per standardizzare la superficie. La superficie risultante viene verificata utilizzando `TSplineSurface.IsStandard`, che conferma che è ora standard.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## File di esempio

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
