<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## In profondità
`Curve.ExtrudeAsSolid (curve, direction)` estrude una curva piana chiusa di input utilizzando un vettore di input per determinare la direzione dell'estrusione. La lunghezza del vettore viene utilizzata per la distanza di estrusione. Questo nodo chiude le estremità dell'estrusione per creare un solido.

Nell'esempio seguente, viene innanzitutto creata una NurbsCurve utilizzando un nodo `NurbsCurve.ByPoints`, con un insieme di punti generati in modo casuale come input. Viene utilizzato un blocco di codice per specificare i componenti X, Y e Z di un nodo `Vector.ByCoordinates`. Questo vettore viene quindi utilizzato come input direction in un nodo `Curve.ExtrudeAsSolid`.
___
## File di esempio

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
