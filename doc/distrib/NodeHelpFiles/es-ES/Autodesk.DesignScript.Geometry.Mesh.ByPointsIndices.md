## En detalle
`Mesh.ByPointsIndices` utiliza una lista de puntos, que representan los vértices de los triángulos de la malla, y una lista de índices, que representan cómo se une la malla, y crea una nueva malla. La entrada `points` debe ser una lista plana de vértices únicos en la malla. La entrada `indices` debe ser una lista plana de enteros. Cada conjunto de tres enteros designa un triángulo en la malla. Los enteros especifican el índice del vértice en la lista de vértices. La entrada `indices` debe estar indexada a 0, con el primer punto de la lista de vértices con el índice 0.

En el ejemplo siguiente, se utiliza un nodo `Mesh.ByPointsIndices` para crear una malla mediante una lista de nueve puntos y una lista de 36 índices, que especifican la combinación de vértices para cada uno de los 12 triángulos de la malla.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
