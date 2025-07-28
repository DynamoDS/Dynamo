<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
Узел `TSplineReflection.SegmentAngle` возвращает угол между каждой парой сегментов радиального отражения. Если отражение TSplineReflection является осевым, узел возвращает 0.

В приведенном ниже примере создается Т-сплайновая поверхность с добавленными отражениями. Далее на графике поверхность исследуется с помощью узла `TSplineSurface.Reflections`. Затем результат (отражение) используется в качестве входных данных для узла `TSplineReflection.SegmentAngle`, который возвращает угол между сегментами радиального отражения.

## Файл примера

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
