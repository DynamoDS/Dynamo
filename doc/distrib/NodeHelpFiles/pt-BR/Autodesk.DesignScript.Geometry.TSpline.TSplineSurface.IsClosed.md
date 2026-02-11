## Em profundidade
Uma superfície fechada é aquela que compõe uma forma completa sem aberturas ou limites.
No exemplo abaixo, uma esfera da T-Spline gerada por meio de `TSplineSurface.BySphereCenterPointRadius` é inspecionada usando `TSplineSurface.IsClosed` para verificar se está aberta, o que retorna um resultado negativo. Isso porque as esferas da T-Spline, embora pareçam fechadas, estão na verdade abertas em polos em que várias arestas e vértices estão empilhados em um ponto.

Em seguida, os intervalos na esfera da T-Spline são preenchidos com o nó `TSplineSurface.FillHole`, que resulta em uma pequena deformação onde a superfície foi preenchida. Quando é verificada novamente através do nó `TSplineSurface.IsClosed`, agora ela gera um resultado positivo, o que significa que está fechado.
___
## Arquivo de exemplo

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
