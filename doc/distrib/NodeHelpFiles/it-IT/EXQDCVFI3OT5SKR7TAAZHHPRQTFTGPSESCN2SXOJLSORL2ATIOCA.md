<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## In profondità
Curve.ExtrudeAsSolid (direction, distance) estrude una curva piana chiusa di input utilizzando un vettore di input per determinare la direzione dell'estrusione. Viene utilizzato un input `distance` distinto per la distanza di estrusione. Questo nodo chiude le estremità dell'estrusione per creare un solido.

Nell'esempio seguente, viene innanzitutto creata un NurbsCurve utilizzando un nodo `NurbsCurve.ByPoints`, con un insieme di punti generati in modo casuale come input. Viene utilizzato un `code block` per specificare i componenti X, Y e Z di un nodo `Vector.ByCoordinates`. Questo vettore viene quindi utilizzato come input direction in un nodo `Curve.ExtrudeAsSolid` mentre viene utilizzato un dispositivo di scorrimento numerico per controllare l'input `distance`.
___
## File di esempio

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
