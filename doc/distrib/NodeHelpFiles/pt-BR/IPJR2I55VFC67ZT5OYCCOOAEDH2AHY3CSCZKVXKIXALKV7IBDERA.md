<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormalXAxis` gera uma superfície de plano de primitivo da T-Spline usando um ponto de origem, um vetor normal e uma direção de vetor do eixo X do plano. Para criar o plano da T-Spline, o nó usa as seguintes entradas:
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
- `xAxis` : um vetor que define a direção do eixo X, permitindo maior controle sobre a orientação do plano criado.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

No exemplo abaixo, é criada uma superfície plana da T-Spline usando o ponto de origem fornecido e a normal que é um vetor do eixo X. A entrada `xAxis` é definida como o eixo Z. O tamanho da superfície é controlado pelos dois pontos usados como entradas `minCorner` e `maxCorner`.

## Arquivo de exemplo

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
