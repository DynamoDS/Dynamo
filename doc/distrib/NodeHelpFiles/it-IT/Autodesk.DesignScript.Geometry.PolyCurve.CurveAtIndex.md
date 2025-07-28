## In profondità
CurveAtIndex restituirà il segmento di curva in corrispondenza dell'indice di input di una data PolyCurve. Se il numero di curve nella PolyCurve è inferiore all'indice specificato, CurveAtIndex restituirà null. L'input endOrStart accetta un valore booleano true o false. Se è true, CurveAtIndex inizierà il conteggio al primo segmento della PolyCurve. Se è false, verrà eseguito all'indietro dall'ultimo segmento. Nell'esempio seguente, viene generato un insieme di punti casuali, quindi viene utilizzato PolyCurve.ByPoints per creare una PolyCurve aperta. È quindi possibile utilizzare CurveAtIndex per estrarre segmenti specifici dalla PolyCurve.
___
## File di esempio

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

