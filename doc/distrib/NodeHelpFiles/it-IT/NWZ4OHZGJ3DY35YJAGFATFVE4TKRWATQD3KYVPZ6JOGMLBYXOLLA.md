<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## In profondità
`Curve.ExtrudeAsSolid (curve, distance)` estrude una curva piana chiusa di input utilizzando un numero di input per determinare la distanza dell'estrusione. La direzione dell'estrusione viene determinata dal vettore normale del piano in cui giace la curva. Questo nodo chiude le estremità dell'estrusione per creare un solido.

Nell'esempio seguente, viene innanzitutto creata una NurbsCurve utilizzando un nodo `NurbsCurve.ByPoints` con un insieme di punti generati in modo casuale come input. Quindi, viene utilizzato un nodo `Curve.ExtrudeAsSolid` per estrudere la curva come solido. Viene utilizzato un dispositivo di scorrimento numerico come input `distance` nel nodo `Curve.ExtrudeAsSolid`.
___
## File di esempio

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
