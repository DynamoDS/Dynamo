## In-Depth
Węzeł `TSplineReflection.ByAxial` zwraca obiekt `TSplineReflection`, którego można używać jako pozycji danych wejściowych węzła `TSplineSurface.AddReflections`.
Pozycja danych wejściowych węzła `TSplineReflection.ByAxial` to płaszczyzna, która służy jako płaszczyzna odbicia. Podobnie jak obiekt TSplineInitialSymmetry, obiekt TSplineReflection po ustaleniu go dla TSplineSurface wpływa na wszystkie kolejne operacje i zmiany.

W poniższym przykładzie za pomocą węzła `TSplineReflection.ByAxial` zostaje utworzony obiekt TSplineReflection umieszczony na górze stożka T-splajn. To odbicie jest następnie używane jako pozycja danych wejściowych węzłów `TSplineSurface.AddReflections`, aby odzwierciedlić stożek i zwrócić nową powierzchnię T-splajn.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
