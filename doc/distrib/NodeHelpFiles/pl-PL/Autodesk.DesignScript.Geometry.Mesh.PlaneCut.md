## Informacje szczegółowe
Węzeł `Mesh.PlaneCut` zwraca siatkę, która została przecięta za pomocą danej płaszczyzny. Wynikiem cięcia jest część siatki leżąca z boku płaszczyzny w kierunku wektora normalnego wejścia `plane`. Parametr `makeSolid` określa, czy siatka jest traktowana jako bryła, `Solid` — w takim przypadku wycięcie jest wypełniane najmniejszą możliwą liczbą trójkątów w celu pokrycia każdego otworu.

W poniższym przykładzie siatka pusta uzyskana za pomocą operacji `Mesh.BooleanDifference` zostaje przecięta płaszczyzną pod kątem.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
