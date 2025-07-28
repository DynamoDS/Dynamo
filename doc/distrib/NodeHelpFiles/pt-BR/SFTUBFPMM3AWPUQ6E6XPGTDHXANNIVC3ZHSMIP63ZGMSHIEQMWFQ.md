<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
`TSplineSurface.ByPlaneThreePoints` gera uma superfície de plano de primitivo da T-Spline usando três pontos como entrada. Para criar o plano da T-Spline, o nó usa as seguintes entradas:
- `p1`, `p2` e `p3`: três pontos que definem a posição do plano. O primeiro ponto é considerado a origem do plano.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

No exemplo abaixo, é criada uma superfície plana da T-Spline usando três pontos gerados aleatoriamente. O primeiro ponto é a origem do plano. O tamanho da superfície é controlado pelos dois pontos usados como entradas `minCorner’ e `maxCorner’.

## Arquivo de exemplo

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
