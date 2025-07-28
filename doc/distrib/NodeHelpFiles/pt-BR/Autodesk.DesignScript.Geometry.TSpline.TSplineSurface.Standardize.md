## Em profundidade
O nó `TSplineSurface.Standardize` é usado para padronizar uma superfície da T-Spline.
Padronizar significa preparar uma superfície da T-Spline para conversão NURBS e implica estender todos os pontos T até que eles sejam separados de pontos de estrela por pelo menos duas isocurvas. Padronizar não altera a forma da superfície, mas pode adicionar pontos de controle para atender aos requisitos de geometria necessários para tornar a superfície NURBS compatível.

No exemplo abaixo, uma superfície da T-Spline gerada por meio de `TSplineSurface.ByBoxLengths` tem uma de suas faces subdivididas.
Um nó `TSplineSurface.IsStandard` é usado para verificar se a superfície é padrão, mas produz um resultado negativo.
`TSplineSurface.Standardize` é usado para padronizar a superfície. A superfície resultante é verificada usando `TSplineSurface.IsStandard`, que confirma que agora é padrão.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Arquivo de exemplo

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
