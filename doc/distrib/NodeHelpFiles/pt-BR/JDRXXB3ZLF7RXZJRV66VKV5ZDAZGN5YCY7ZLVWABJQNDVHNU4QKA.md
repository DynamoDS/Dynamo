<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis --->
<!--- JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA --->
## In-Depth
`TSplineSurface.ByPlaneOriginXAxisYAxis` gera uma superfície de plano de primitivo da T-Spline usando um ponto de origem e dois vetores representando os eixos X e Y do plano. Para criar o plano da T-Spline, o nó usa as seguintes entradas:
- `origin`: a point defining the origin of the plane.
- `xAxis` e `yAxis`: vetores que definem a direção dos eixos X e Y do plano criado.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

No exemplo abaixo, é criada uma superfície plana da T-Spline usando o ponto de origem fornecido e dois vetores servindo como direções X e Y. O tamanho da superfície é controlado pelos dois pontos usados como entradas `minCorner` e `maxCorner`.

## Arquivo de exemplo

![Example](./JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA_img.jpg)
