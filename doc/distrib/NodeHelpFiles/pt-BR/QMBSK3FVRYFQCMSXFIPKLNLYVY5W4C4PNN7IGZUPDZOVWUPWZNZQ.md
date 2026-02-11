<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints` gera uma superfície de plano de primitivo da T-Spline com base em uma lista de pontos. Para criar o plano da T-Spline, o nó usa as seguintes entradas:
- `points`: um conjunto de pontos para definir a orientação e origem do plano. Nos casos em que os pontos de entrada não se encontram em um único plano, a orientação do plano é determinada com base no melhor ajuste. É necessário um mínimo de três pontos para criar a superfície.
- `minCorner` e `maxCorner`: os cantos do plano, representados como pontos com valores X e Y (as coordenadas Z serão ignoradas). Esses cantos representarão as extensões da superfície da T-Spline de saída se ela for convertida no plano XY. Os pontos `minCorner` e `maxCorner` não precisam coincidir com os vértices de canto em 3D.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

No exemplo abaixo, é criada uma superfície plana da T-Spline usando uma lista de pontos gerada aleatoriamente. O tamanho da superfície é controlado pelos dois pontos usados como entradas `minCorner` e `maxCorner`.

## Arquivo de exemplo

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
