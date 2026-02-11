<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## Подробности
`NurbsCurve.ByControlPointsWeightsKnots` позволяет вручную управлять весом и узлами объекта NurbsCurve. Список весов должен иметь ту же длину, что и список контрольных точек. Длина списка узлов должна быть равна количеству контрольных точек плюс градус плюс один.

В примере ниже сначала создается объект NurbsCurve путем интерполяции между рядом произвольных точек. Узлы, веса и контрольные точки используются для поиска соответствующих частей этой кривой. Мы можем использовать `List.ReplaceItemAtIndex` для изменения списка весов. С помощью `NurbsCurve.ByControlPointsWeightsKnots` можно воссоздать объект NurbsCurve с измененными весами.

___
## Файл примера

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

