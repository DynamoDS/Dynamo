## Informacje szczegółowe
Węzeł `Mesh.Nearest` zwraca punkt na siatce wejściowej, który jest najbliższy danemu punktowi. Zwrócony punkt jest rzutowaniem punktu wejściowego na siatkę przy użyciu wektora normalnego do siatki przechodzącej przez ten punkt, co daje najbliższy możliwy punkt.

W poniższym przykładzie utworzono prosty przypadek użycia, aby pokazać, jak działa ten węzeł. Punkt wejściowy znajduje się powyżej sferycznej siatki, ale nie bezpośrednio na jej górze. Punkt wynikowy jest najbliższym punktem leżącym na siatce. Kontrastuje to z danymi wyjściowymi węzła `Mesh.Project` (w przypadku użycia tego samego punktu i siatki jako danych wejściowych oraz wektora w kierunku ujemnym „Z”), w przypadku którego punkt wynikowy jest rzutowany na siatkę bezpośrednio poniżej punktu wejściowego. Węzeł `Line.ByStartAndEndPoint` służy do wyświetlania „trajektorii” punktu rzutowanego na siatkę.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Nearest_img.jpg)
