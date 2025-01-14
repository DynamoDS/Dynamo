## In-Depth
`TSplineReflection.ByRadial` retorna um objeto `TSplineReflection` que pode ser usado como entrada para o nó `TSplineSurface.AddReflections`. O nó assume um plano como entrada, e a normal do plano atua como o eixo para rotacionar a geometria. Assim como TSplineInitialSymmetry, TSplineReflection, depois de estabelecido na criação de TSplineSurface, influencia todas as operações e alterações subsequentes.

No exemplo abaixo, `TSplineReflection.ByRadial` é usado para definir a reflexão de uma superfície da T-Spline. As entradas `segmentsCount` e `segmentAngle` são usadas para controlar a forma como a geometria é refletida em torno da normal de um determinado plano. A saída do nó é usada como entrada para o nó `TSplineSurface.AddReflections` para criar uma nova superfície da T-Spline.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
