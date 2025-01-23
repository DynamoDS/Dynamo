<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## Подробности
`PolyCurve.ByGroupedCurves` создает новый объект PolyCurve путем группировки нескольких соединенных кривых. Узел группирует кривые на основе их близости к другим кривым, соприкасающимся или находящимся в пределах заданного допуска соединения, для объединения их в один объект PolyCurve.

В примере ниже пятиугольник расчленяется, а его кривые распределяются произвольно. Затем для их группировки в объект PolyCurve используется узел `PolyCurve.ByGroupedCurves`.
___
## Файл примера

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
