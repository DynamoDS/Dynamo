<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCoordinateSystemRadii --->
<!--- TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ --->
## In-Depth
В приведенном ниже примере создается Т-сплайновая поверхность тора в заданной системе координат `cs`. Малый и большой радиусы формы задаются входными параметрами `innerRadius` и `outerRadius`. Значения параметров `innerRadiusSpans` и `outerRadiusSpans` управляют определением поверхности по двум направлениям. Исходная симметрия формы задается входным параметром `symmetry`. Если осевая симметрия, примененная к форме, активна для оси X или Y, значение параметра `outerRadiusSpans` тора должно быть кратно 4. Радиальная симметрия не имеет такого требования. Входной параметр `inSmoothMode` используется для переключения между режимами сглаживания и рамки при предварительном просмотре Т-сплайновой поверхности.

## Файл примера

![Example](./TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ_img.jpg)
