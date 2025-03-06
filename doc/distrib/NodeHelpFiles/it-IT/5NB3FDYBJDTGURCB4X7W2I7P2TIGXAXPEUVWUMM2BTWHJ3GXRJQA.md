<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## In profondità
`Curve.Extrude (curve, direction, distance)` estrude una curva di input utilizzando un vettore di input per determinare la direzione dell'estrusione. Viene utilizzato un input `distance` distinto per la distanza di estrusione.

Nell'esempio seguente, viene innanzitutto creata un NurbsCurve utilizzando un nodo `NurbsCurve.ByControlPoints`, con un insieme di punti generati in modo casuale come input. Viene utilizzato Un blocco di codice per specificare i componenti X, Y e Z di un nodo `Vector.ByCoordinates`. Questo vettore viene quindi utilizzato come input direction in un nodo `Curve.Extrude` mentre viene utilizzato un `number slider` per controllare l'input `distance`.
___
## File di esempio

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
