## In-Depth
Узел `TSplineReflection.ByRadial` возвращает объект `TSplineReflection`, который можно использовать в качестве входных данных для узла `TSplineSurface.AddReflections`. Узел принимает плоскость в качестве входных данных, при этом нормаль к плоскости выступает в роли оси, вокруг которой выполняется поворот геометрии. Как и объект TSplineInitialSymmetry, объект TSplineReflection, единожды определенный при создании TSplineSurface, влияет на все последующие операции и изменения.

В приведенном ниже примере узел `TSplineReflection.ByRadial` используется для задания отражения Т-сплайновой поверхности. Входные параметры `segmentsCount` и `segmentAngle` управляют отражением геометрии относительно нормали к заданной плоскости. Затем выходной параметр узла используется в качестве входных данных для узла `TSplineSurface.AddReflections` с целью создания новой Т-сплайновой поверхности.

## Файл примера

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
