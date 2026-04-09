## Em profundidade
Use `NurbsCurve.PeriodicKnots` quando precisar exportar uma curva NURBS fechada para outro sistema (por exemplo, Alias) ou quando esse sistema esperar a curva em sua forma periódica. Muitas ferramentas de CAD esperam essa forma para precisão da conversão.

`PeriodicKnots` retorna o vetor de nós na forma *periódica* (sem restrições). `Knots`  o retorna na forma *fixa*. Ambos os vetores têm o mesmo comprimento; são duas maneiras diferentes de descrever a mesma curva. Na forma fixa, os nós se repetem no início e no fim, de modo que a curva fica fixada no intervalo de parâmetros. Na forma periódica, o espaçamento entre os nós se repete no início e no fim, o que resulta em um laço fechado suave.

No exemplo abaixo, uma curva NURBS periódica é criada com `NurbsCurve.ByControlPointsWeightsKnots`. Observe os nós que comparam `Knots` e `PeriodicKnots` para ver o mesmo comprimento, mas valores diferentes. `Knots` é a forma fixa (nós repetidos nas extremidades) e `PeriodicKnots` é a forma não fixa com o padrão de diferença repetido que define a periodicidade da curva.
___
## Arquivo de exemplo

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
