## In profondità
ContainmentTest restituisce un valore booleano a seconda che un determinato punto sia contenuto o meno all'interno di un determinato poligono. Affinché tutto questo funzioni, il poligono deve essere piano e non autointersecante. Nell'esempio seguente, viene creato un poligono utilizzando una serie di punti creati da coordinate cilindriche. Lasciando costante la quota altimetrica e ordinando gli angoli si garantisce un poligono piano e non autointersecante. Si crea quindi un punto da verificare e si utilizza ContainmentTest per controllare se il punto si trova all'interno o all'esterno del poligono.
___
## File di esempio

![ContainmentTest](./Autodesk.DesignScript.Geometry.Polygon.ContainmentTest_img.jpg)

