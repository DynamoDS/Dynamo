## In profondità
SelfIntersections restituirà un elenco di tutti i punti dove un poligono si autointerseca. Nell'esempio seguente, viene prima generato un elenco di angoli e raggi casuali non ordinati da utilizzare con Point.ByCylindricalCoordinates. Poiché è stata mantenuta costante la quota altimetrica e non è stato eseguito l'ordinamento degli angoli di tali punti, un poligono creato con Polygon.ByPoints sarà piano e probabilmente autointersecante. È quindi possibile trovare i punti di intersezione utilizzando SelfIntersections.
___
## File di esempio

![SelfIntersections](./Autodesk.DesignScript.Geometry.Polygon.SelfIntersections_img.jpg)

