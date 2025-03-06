## En detalle:
`TSplineSurface.Thicken(distance, softEdges)` engrosa una superficie de T-Spline hacia fuera (o hacia dentro, cuando se proporciona un valor negativo para `distance`) con el valor especificado para `distance` a lo largo de las normales de las caras. La entrada booleana `softEdges` determina si las aristas resultantes se suavizan ("true") o se pliegan ("false").

En el ejemplo siguiente, una superficie cilíndrica de T-Spline se engrosa mediante el nodo `TSplineSurface.Thicken(distance, softEdges)`. La superficie resultante se traslada a un lado para visualizarla mejor.
___
## Archivo de ejemplo

![TSplineSurface.Thicken](./UHLOMXPCNY3C36FQ45G3HQGKIZLSUE2QX4N7FY7ZCCOEN7F7Q6YA_img.jpg)
