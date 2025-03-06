## Em profundidade
Curve At Index retornará o segmento de curva no índice de entrada de uma PolyCurve fornecida. Se o número de curvas na PolyCurve for menor do que o índice fornecido, CurveAtIndex retornará nulo. A entrada endOrStart aceita um valor booleano True ou False. Se for True, CurveAtIndex começará a contar no primeiro segmento da PolyCurve. Se for False, contará para trás desde o último segmento. No exemplo abaixo, geramos um conjunto de pontos aleatórios e, em seguida, usamos PolyCurve By Points para criar uma PolyCurve aberta. Em seguida, podemos usar CurveAtIndex para extrair segmentos específicos da PolyCurve.
___
## Arquivo de exemplo

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

