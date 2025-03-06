## In-Depth
No exemplo abaixo, é criada uma superfície da T-Spline como uma extrusão de uma entrada `curve` de perfil determinada. A curva pode ser aberta ou fechada. A extrusão é realizada em uma determinada `direction` e pode estar em ambas as direções, controlada por entradas `frontDistance` e `backDistance`. Os vãos podem ser definidos individualmente para as duas direções de extrusão, com os `frontSpans` e `backSpans` fornecidos. Para estabelecer a definição da superfície ao longo da curva, `profileSpans` controla o número de faces e `uniform` as distribui de forma uniforme ou leva em conta a curvatura. Por fim, `inSmoothMode` controla se a superfície é exibida em modo suave ou de caixa.

## Arquivo de exemplo
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
