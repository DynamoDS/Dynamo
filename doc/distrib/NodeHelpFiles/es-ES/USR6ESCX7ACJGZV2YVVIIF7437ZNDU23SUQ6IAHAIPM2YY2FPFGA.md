## En detalle:
`TSplineSurface.Thicken(vector, softEdges)` engrosa una superficie de T-Spline guiada por el vector especificado. La operación de engrosamiento duplica la superficie en la dirección del `vector` y, a continuación, conecta las dos superficies mediante la unión de sus aristas. La entrada booleana `softEdges` determina si las aristas resultantes se suavizan ("true") o se pliegan ("false").

En el ejemplo siguiente, una superficie extruida de T-Spline se engrosa mediante el nodo `TSplineSurface.Thicken(vector, softEdges)`. La superficie resultante se traslada a un lado para visualizarla mejor.


___
## Archivo de ejemplo

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
