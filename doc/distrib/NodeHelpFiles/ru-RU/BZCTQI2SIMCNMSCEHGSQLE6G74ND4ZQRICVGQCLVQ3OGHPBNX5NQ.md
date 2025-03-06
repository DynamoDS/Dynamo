<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## Подробности
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` сначала разделяет кривую в точках, определенных входным списком параметров. Затем узел возвращает либо нечетные, либо четные нумерованные сегменты, как определено логическим значением входных данных `discardEvenSegments`.

В примере ниже сначала создается объект NurbsCurve с помощью узла `NurbsCurve.ByControlPoints`, в котором набор точек генерируется случайным образом. Для создания диапазона чисел от 0 до 1 с шагом 0,1 используется `code block`. При использовании диапазона в качестве входных параметров для узла `Curve.TrimSegmentsByParameter` создается список кривых, которые фактически являются штриховой версией исходной кривой.
___
## Файл примера

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
