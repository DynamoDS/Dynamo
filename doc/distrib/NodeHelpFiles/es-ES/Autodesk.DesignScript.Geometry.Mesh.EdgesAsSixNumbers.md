## En detalle
`Mesh.EdgesAsSixNumbers` determina las coordenadas X, Y y Z de los vértices que componen cada arista única en una malla proporcionada, dando como resultado seis números por arista. Este nodo puede utilizarse para consultar o reconstruir la malla o sus aristas.

En el ejemplo siguiente, se utiliza `Mesh.Cuboid` para crear una malla de ortoedro, que se usa a continuación como entrada al nodo `Mesh.EdgesAsSixNumbers` para recuperar la lista de aristas expresadas como seis números. La lista se subdivide en listas de seis elementos mediante `List.Chop`; a continuación, se utilizan `List.GetItemAtIndex` y `Point.ByCoordinates` para reconstruir las listas de puntos iniciales y finales de cada arista. Por último, se utiliza `List.ByStartPointEndPoint` para reconstruir las aristas de la malla.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
