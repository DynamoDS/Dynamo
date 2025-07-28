<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
Узел `TSplineReflection.SegmentsCount` возвращает количество сегментов радиального отражения. Если отражение TSplineReflection является осевым, узел возвращает значение 0.

В приведенном ниже примере создается Т-сплайновая поверхность с добавленными отражениями. Далее на графике поверхность исследуется с помощью узла `TSplineSurface.Reflections`. Затем результат (отражение) используется в качестве входного параметра для узла `TSplineReflection.SegmentsCount`, который возвращает количество сегментов радиального отражения, использованного для создания Т-сплайновой поверхности.

## Файл примера

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
