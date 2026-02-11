## In profondità
PlaneDeviation calcolerà prima il piano di adattamento attraverso i punti di un determinato poligono. Calcola quindi la distanza di ogni punto rispetto a tale piano per trovare la deviazione massima dei punti dal piano di adattamento. Nell'esempio seguente, viene generato un elenco di angoli, quote altimetriche e raggi casuali e si utilizza quindi Point.ByCylindricalCoordinates per creare un insieme di punti non piani da utilizzare per Polygon.ByPoints. Inserendo questo poligono in PlaneDeviation, è possibile trovare la deviazione media dei punti da un piano di adattamento.
___
## File di esempio

![PlaneDeviation](./Autodesk.DesignScript.Geometry.Polygon.PlaneDeviation_img.jpg)

