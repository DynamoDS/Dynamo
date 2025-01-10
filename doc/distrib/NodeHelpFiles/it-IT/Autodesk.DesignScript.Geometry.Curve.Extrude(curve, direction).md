## In profondità
`Curve.Extrude (curve, direction)` estrude una curva di input utilizzando un vettore di input per determinare la direzione dell'estrusione. La lunghezza del vettore viene utilizzata per la distanza di estrusione.

Nell'esempio seguente, viene innanzitutto creata un NurbsCurve utilizzando un nodo `NurbsCurve.ByControlPoints`, con un insieme di punti generati in modo casuale come input. Viene utilizzato un blocco di codice per specificare i componenti X, Y e Z di un nodo `Vector.ByCoordinates`. Questo vettore viene quindi utilizzato come input `direction` in un nodo `Curve.Extrude`.
___
## File di esempio

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
