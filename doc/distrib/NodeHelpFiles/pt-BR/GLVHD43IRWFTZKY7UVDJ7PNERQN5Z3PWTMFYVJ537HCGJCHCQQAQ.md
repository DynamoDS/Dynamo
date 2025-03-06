<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
`TSplineReflection.SegmentsCount` retorna o número de segmentos de uma reflexão radial. Se o tipo de TSplineReflection for Axial, o nó retornará um valor de 0.

No exemplo abaixo, é criada uma superfície da T-Spline com reflexões adicionadas. Mais tarde no gráfico, a superfície é interrogada com o nó `TSplineSurface.Reflections`. O resultado (uma reflexão) é usado como entrada para o `TSplineReflection.SegmentsCount` retornar o número de segmentos de uma reflexão radial que foi usado para criar a superfície da T-Spline.

## Arquivo de exemplo

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
