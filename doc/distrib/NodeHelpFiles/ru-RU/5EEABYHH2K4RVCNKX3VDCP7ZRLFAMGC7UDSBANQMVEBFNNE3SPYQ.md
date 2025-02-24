<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## Подробности
`Curve.NormalAtParameter (curve, param)` возвращает вектор, выровненный по направлению нормали с заданным параметром кривой. Параметризация кривой измеряется в диапазоне от 0 до 1, где 0 представляет начало кривой, а 1 — ее конец.

В примере ниже сначала создается объект NurbsCurve с помощью узла `NurbsCurve.ByControlPoints`, в котором набор точек генерируется случайным образом. Для управления вводом значения `parameter` для узла `Curve.NormalAtParameter` используется регулятор чисел с диапазоном от 0 до 1.
___
## Файл примера

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
