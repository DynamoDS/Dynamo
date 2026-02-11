## In profondità
`Curve.Extrude (curve, distance)` estrude una curva di input utilizzando un numero di input per determinare la distanza dell'estrusione. La direzione del vettore normale lungo la curva viene utilizzata per la direzione di estrusione.

Nell'esempio seguente, viene innanzitutto creata una NurbsCurve utilizzando un nodo `NurbsCurve.ByControlPoints`, con un insieme di punti generati in modo casuale come input. Quindi, viene utilizzato un nodo `Curve.Extrude` per estrudere la curva. Viene utilizzato un dispositivo di scorrimento numerico come input `distance` nel nodo `Curve.Extrude`.
___
## File di esempio

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
