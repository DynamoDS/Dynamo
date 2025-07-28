## In profondità
`Cuboid.Height` restituisce l'altezza del cuboide di input. Tenere presente che se il cuboide è stato trasformato in un sistema di coordinate diverso con un fattore di scala, verranno restituite le quote originali del cuboide, non le quote dello spazio globale. In altre parole, se si crea un cuboide con una larghezza (asse X) di 10 e lo si trasforma in un CoordinateSystem con una scala pari a 2 volte in X, la larghezza sarà ancora 10.

Nell'esempio seguente, viene generato un cuboide mediante angoli, quindi viene utilizzato un nodo `Cuboid.Height` per trovarne l'altezza.

___
## File di esempio

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

