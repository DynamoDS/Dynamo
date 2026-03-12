## Em profundidade
Use `NurbsCurve.PeriodicControlPoints` quando precisar exportar uma curva NURBS fechada para outro sistema (por exemplo, Alias) ou quando esse sistema esperar a curva em sua forma periódica. Muitas ferramentas de CAD esperam essa forma para precisão da conversão.

`PeriodicControlPoints` retorna os pontos de controle na forma *periódica*. `ControlPoints` os retorna na forma *fixa*. Ambas as matrizes têm o mesmo número de pontos; são duas maneiras diferentes de descrever a mesma curva. Na forma periódica, os últimos pontos de controle coincidem com os primeiros (tantos quantos forem os graus da curva), de modo que a curva se fecha suavemente. A forma fixa usa um layout diferente, portanto as posições dos pontos nas duas matrizes diferem.

No exemplo abaixo, uma curva NURBS periódica é criada com `NurbsCurve.ByControlPointsWeightsKnots`. Observe os nós que comparam `ControlPoints` e `PeriodicControlPoints` para ver que ambos têm o mesmo comprimento, mas posições diferentes. Os `ControlPoints` são exibidos em vermelho para se destacarem dos `PeriodicControlPoints`, que são pretos, na pré-visualização em segundo plano.
___
## Arquivo de exemplo

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
