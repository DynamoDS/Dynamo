## In profondità
CloseWithLine aggiunge una linea retta tra il punto iniziale e il punto finale di una PolyCurve aperta. Restituisce una nuova PolyCurve che include la linea aggiunta. Nell'esempio seguente, viene generato un insieme di punti casuali e si utilizza PolyCuve.ByPoints con l'input connectLastToFirst impostato su false per creare una PolyCurve aperta. L'inserimento di questa PolyCurve in CloseWithLine crea una nuova PolyCurve chiusa (in questo caso sarebbe equivalente all'utilizzo di un input "true" per l'opzione connectLastToFirst in PolyCurve.ByPoints).
___
## File di esempio

![CloseWithLine](./Autodesk.DesignScript.Geometry.PolyCurve.CloseWithLine_img.jpg)

