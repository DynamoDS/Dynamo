## In-Depth
Узел `TSplineReflection.ByAxial` возвращает объект `TSplineReflection`, который можно использовать в качестве входных данных для узла `TSplineSurface.AddReflections`.
Ввод узла `TSplineReflection.ByAxial` представляет собой плоскость, которая выступает в роли плоскости отражения. Как и объект TSplineInitialSymmetry, объект TSplineReflection, единожды определенный для TSplineSurface, влияет на все последующие операции и изменения.

В приведенном ниже примере для создания объекта TSplineReflection, расположенного в верхней части Т-сплайнового конуса, используется узел `TSplineReflection.ByAxial`. Затем отражение используется в качестве входных данных в узлах `TSplineSurface.AddReflections` для отражения конуса и возврата новой T-сплайновой поверхности.

## Файл примера

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
