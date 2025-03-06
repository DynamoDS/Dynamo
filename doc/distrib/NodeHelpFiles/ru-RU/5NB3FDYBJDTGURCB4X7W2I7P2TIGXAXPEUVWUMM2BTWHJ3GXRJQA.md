<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## Подробности
`Curve.Extrude (curve, direction, distance)` выдавливает входную кривую с помощью входного вектора для определения направления выдавливания. В качестве расстояния выдавливания используется отдельное значение `distance`.

В примере ниже сначала создается объект NurbsCurve с помощью узла `NurbsCurve.ByControlPoints`, в котором входной набор точек генерируется случайным образом. Для задания компонентов X, Y и Z узла `Vector.ByCoordinates` используется блок кода. Этот вектор затем используется как входное значение направления в узле `Curve.Extrude`, а для управления входным значением `distance` используется `number slider`.
___
## Файл примера

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
