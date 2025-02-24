## Informacje szczegółowe
Węzeł `Mesh.Sphere` tworzy sferę (siatkę) wyśrodkowaną w punkcie wejściowym `origin` o danym promieniu `radius` i liczbie podziałów `divisions`. Wejściowa wartość logiczna `icosphere` służy do przełączania między typami siatki sferycznej `icosphere` i `UV-Sphere`. Siatka ikosfery pokrywa sferę większą liczbą regularnych trójkątów niż siatka UV i zwykle daje lepsze wyniki w kolejnych operacjach modelowania. Siatka UV ma bieguny wyrównane z osią sfery, a warstwy trójkątów są generowane podłużnie wokół osi.

W przypadku ikosfery liczba trójkątów wokół osi sfery może być tak mała, jak określona liczba podziałów, i co najwyżej dwa razy większa. Podziały sfery `UV-sphere` określają liczbę warstw trójkątów generowanych podłużnie wokół sfery. Gdy wartość wejściowa `divisions` jest ustawiona na zero, węzeł zwraca sferę UV z domyślną liczbą 32 podziałów dla każdego typu siatki.

W poniższym przykładzie węzeł `Mesh.Sphere` służy do utworzenia dwóch sfer o identycznym promieniu i podziałach, ale przy użyciu różnych metod. Gdy pozycja danych wejściowych `icosphere` jest ustawiona na wartość `True`, węzeł `Mesh.Sphere` zwraca ikosferę — `icosphere`. Natomiast gdy pozycja danych wejściowych `icosphere` jest ustawiona na wartość `False`, węzeł `Mesh.Sphere` zwraca sferę UV — `UV-sphere`.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
