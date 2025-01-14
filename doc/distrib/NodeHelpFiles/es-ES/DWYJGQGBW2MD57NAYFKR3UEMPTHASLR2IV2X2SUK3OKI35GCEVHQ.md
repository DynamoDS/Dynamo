<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormal` genera una superficie de plano de primitiva de T-Spline mediante un punto de origen y un vector normal. Para crear el plano de T-Spline, el nodo utiliza las siguientes entradas:
- `origin`: un punto que define el origen del plano.
- `normal`: un vector que especifica la dirección normal del plano creado.
- `minCorner` y `maxCorner`: las esquinas del plano, representadas como puntos con valores X e Y (se omitirán las coordenadas Z). Estas esquinas representan la extensión de la superficie de T-Spline de salida si se traslada en el plano XY. Los puntos `minCorner` y `maxCorner` no tienen que coincidir con los vértices de esquina en 3D. Por ejemplo, cuando el valor de `minCorner` es (0,0) y el de `maxCorner` es (5,10), la anchura y la longitud del plano serán 5 y 10 respectivamente.
- `xSpans` y `ySpans`: número de tramos/divisiones de anchura y longitud del plano.
- `symmetry`: determina si la geometría es simétrica con respecto a sus ejes X, Y y Z
- `inSmoothMode`: determina si la geometría resultante aparece en el modo de cuadro o suavizado.

En el ejemplo siguiente, se crea una superficie plana de T-Spline mediante el punto de origen proporcionado y la normal, que es un vector del eje X. El tamaño de la superficie se controla mediante los dos puntos utilizados como entradas `minCorner` y `maxCorner`.

## Archivo de ejemplo

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
