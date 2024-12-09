## In profondità
Una superficie chiusa è quella che assume una forma completa, senza aperture o contorni.
Nell'esempio seguente, una sfera T-Spline generata tramite `TSplineSurface.BySphereCenterPointRadius` viene ispezionata utilizzando `TSplineSurface.IsClosed` per verificare se è aperta, il che restituisce un risultato negativo. Ciò perché le sfere T-Spline, sebbene siano chiuse, sono effettivamente aperte ai poli in cui più bordi e vertici si impilano in un punto.

Gli spazi nella sfera T-Spline vengono quindi riempiti utilizzando il nodo `TSplineSurface.FillHole`, che produce una leggera deformazione dove la superficie è stata riempita. Quando viene nuovamente controllata attraverso il nodo `TSplineSurface.IsClosed`, ora produce un risultato positivo, ovvero risulta chiusa.
___
## File di esempio

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
