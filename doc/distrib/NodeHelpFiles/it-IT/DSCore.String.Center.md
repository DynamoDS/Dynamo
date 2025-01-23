## In profondità
Polygon.Center trova il centro di un determinato poligono utilizzando il valore medio degli angoli. Per i poligoni concavi, è possibile che il centro si trovi effettivamente all'esterno del poligono. Nell'esempio seguente, viene prima generato un elenco di angoli e raggi casuali da utilizzare come input in Point.ByCylindricalCoordinates. Ordinando prima gli angoli, si garantisce che il poligono risultante verrà connesso in ordine di angolo crescente e pertanto che non sarà autointersecante. È quindi possibile utilizzare Center per calcolare la media dei punti e trovare il centro del poligono.
___
## File di esempio

![Center](./DSCore.String.Center_img.jpg)

