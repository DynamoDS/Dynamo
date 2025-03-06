## En detalle
`Mesh.Cone` crea un cono de malla cuya base está centrada en un punto de origen de entrada con un valor de entrada para los radios de la base y la parte superior, la altura y un número de divisiones. El número de la entrada `divisions` se corresponde con el número de vértices que se crean en la parte superior y en la base del cono. Si el número de divisiones es 0, Dynamo utiliza un valor por defecto. El número de divisiones a lo largo del eje Z es siempre igual a 5. La entrada `cap` utiliza un valor booleano para controlar si el cono se cierra en la parte superior.
En el ejemplo siguiente, el nodo `Mesh.Cone` se utiliza para crear una malla en forma de cono con seis divisiones, por lo que la base y la parte superior del cono son hexágonos. El nodo `Mesh.Triangles` se utiliza para visualizar la distribución de los triángulos de la malla.


## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
