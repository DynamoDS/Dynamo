## Informacje szczegółowe
Węzeł `Mesh.Project` zwraca punkt na siatce wejściowej, który jest rzutowaniem punktu wejściowego na siatkę w kierunku danego wektora. Aby węzeł działał poprawnie, linia narysowana od punktu wejściowego w kierunku wektora wejściowego powinna przecinać się z dostarczoną siatką.

Przykładowy wykres przedstawia prosty przypadek użycia tego węzła. Punkt wejściowy znajduje się powyżej sferycznej siatki, ale nie bezpośrednio na jej górze. Punkt jest rzutowany w kierunku ujemnego wektora osi Z. Punkt wynikowy jest rzutowany na sferę i pojawia się tuż pod punktem wejściowym. Kontrastuje to z danymi wyjściowymi węzła `Mesh.Nearest` (używającego tego samego punktu i siatki jako danych wejściowych), w przypadku którego wynikowy punkt leży na siatce wzdłuż wektora normalnego przechodzącego przez punkt wejściowy (najbliższy punkt). Węzeł `Line.ByStartAndEndPoint` służy do wyświetlania „trajektorii” punktu rzutowanego na siatkę.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Project_img.jpg)
