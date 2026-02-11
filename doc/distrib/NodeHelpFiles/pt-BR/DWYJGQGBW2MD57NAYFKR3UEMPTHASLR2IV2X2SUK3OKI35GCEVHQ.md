<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormal` gera uma superfície de plano de primitivo da T-Spline usando um ponto de origem e um vetor normal. Para criar o plano da T-Spline, o nó usa as seguintes entradas:
- `origin`: um ponto que define a origem do plano.
- `normal`: um vetor que especifica a direção da normal do plano criado.
- `minCorner` e `maxCorner`: os cantos do plano, representados como pontos com valores X e Y (as coordenadas Z serão ignoradas). Esses cantos representarão a extensão da superfície da T-Spline de saída se ela tiver sido convertida no plano XY. Os pontos `minCorner` e `maxCorner` não precisam coincidir com os vértices de canto em 3D. Por exemplo, quando um `minCorner` for definido como (0,0) e `maxCorner` for (5,10), a largura e o comprimento do plano serão 5 e 10, respectivamente.
- `xSpans` e `ySpans`: número de vãos/divisões de largura e comprimento do plano
- `symmetry`: se a geometria é simétrica em relação aos eixos X, Y e Z
- `inSmoothMode`: se a geometria resultante será exibida com o modo suave ou de caixa

No exemplo abaixo, é criada uma superfície plana da T-Spline usando o ponto de origem fornecido e a normal que é um vetor do eixo X. O tamanho da superfície é controlado pelos dois pontos usados como entradas `minCorner` e `maxCorner`.

## Arquivo de exemplo

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
