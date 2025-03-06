## En detalle:
`TSplineSurface.FlattenVertices(vertices, parallelPlane)` modifica las posiciones de los puntos de control de un conjunto especificado de vértices alineándolos con un `parallelPlane` especificado como entrada.

En el ejemplo siguiente, los vértices de una superficie plana de T-Spline se desplazan mediante los nodos `TsplineTopology.VertexByIndex` y `TSplineSurface.MoveVertices`. A continuación, la superficie se desplaza a un lado para obtener una mejor vista preliminar y se utiliza como entrada para un nodo `TSplineSurface.FlattenVertices(vertices, parallelPlane)`. El resultado es una nueva superficie con los vértices seleccionados aplanados en el plano especificado.
___
## Archivo de ejemplo

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
