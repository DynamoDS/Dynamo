## In profondità
Una superficie T-Spline è standard quando tutti i punti a T sono separati da punti a stella mediante almeno due isocurve. È necessaria la standardizzazione per convertire una superficie T-Spline in una superficie NURBS.

Nell'esempio seguente, una superficie T-Spline generata tramite `TSplineSurface.ByBoxLengths` ha una delle facce suddivise. `TSplineSurface.IsStandard` viene utilizzato per verificare se la superficie è standard, ma produce un risultato negativo.
`TSplineSurface.Standardize` viene quindi utilizzato per standardizzare la superficie. Vengono introdotti nuovi punti di controllo senza alterare la forma della superficie. La superficie risultante viene verificata utilizzando `TSplineSurface.IsStandard`, che conferma che è ora standard.
I nodi `TSplineFace.UVNFrame` e `TSplineUVNFrame.Position` vengono utilizzati per evidenziare la faccia suddivisa nella superficie.
___
## File di esempio

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
