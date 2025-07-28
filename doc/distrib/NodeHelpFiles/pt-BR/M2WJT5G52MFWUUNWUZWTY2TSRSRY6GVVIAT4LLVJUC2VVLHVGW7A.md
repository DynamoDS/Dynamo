<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
`TSplineReflection.SegmentAngle` retorna o ângulo entre cada par de segmentos de reflexão radial. Se o tipo de TSplineReflection for Axial, o nó retornará 0.

No exemplo abaixo, é criada uma superfície da T-Spline com reflexões adicionadas. Posteriormente no gráfico, a superfície é interrogada com o nó `TSplineSurface.Reflections`. O resultado (uma reflexão) é usado como entrada para o `TSplineReflection.SegmentAngle` para retornar o ângulo entre os segmentos de uma reflexão radial.

## Arquivo de exemplo

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
