## In-Depth
`TSplineReflection.ByAxial` retorna um objeto `TSplineReflection` que pode ser usado como entrada para o nó `TSplineSurface.AddReflections`.
A entrada do nó `TSplineReflection.ByAxial` é um plano que serve como um plano de espelho. Assim como TSplineInitialSymmetry, TSplineReflection, depois de estabelecido para o TSplineSurface, influencia todas as operações e alterações subsequentes.

No exemplo abaixo, `TSplineReflection.ByAxial` é usado para criar um TSplineReflection posicionado no topo do cone da T-Spline. A reflexão é usada como entrada para os nós `TSplineSurface.AddReflections` para refletir o cone e retornar uma nova superfície da T-Spline.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
