## Em profundidade
A superfície da T-Spline é padrão quando todos os pontos T são separados dos pontos de estrela por pelo menos duas isocurvas. A padronização é necessária para converter uma superfície da T-Spline em uma superfície NURBS.

No exemplo abaixo, uma superfície da T-Spline gerada por meio de `TSplineSurface.ByBoxLengths` tem uma de suas faces subdivididas. `TSplineSurface.IsStandard` é usado para verificar se a superfície é padrão, mas produz um resultado negativo.
`TSplineSurface.Standardize` é usado para padronizar a superfície. Novos pontos de controle são introduzidos sem alterar a forma da superfície. A superfície resultante é verificada usando `TSplineSurface.IsStandard`, que confirma que agora é padrão.
Os nós `TSplineFace.UVNFrame` e `TSplineUVNFrame.Position` são usados para realçar a face subdividida na superfície.
___
## Arquivo de exemplo

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
