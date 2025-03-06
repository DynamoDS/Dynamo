## Informacje szczegółowe
Węzeł `Mesh.Cone` tworzy stożek (siatkę), którego podstawa jest wyśrodkowana w wejściowym punkcie początkowym, z wartościami wejściowymi promienia podstawy i góry, wysokości oraz liczby podziałów (`divisions`). Liczba podziałów odpowiada liczbie wierzchołków utworzonych na górze i w podstawie stożka. Jeśli liczba podziałów wynosi 0, dodatek Dynamo używa wartości domyślnej. Liczba podziałów wzdłuż osi Z jest zawsze równa 5. Pozycja danych wejściowych `cap` przyjmuje wartość logiczną (`Boolean`) do sterowania tym, czy stożek jest zamknięty u góry.
W poniższym przykładzie węzeł `Mesh.Cone` służy do utworzenia siatki w kształcie stożka z 6 podziałami, a więc podstawa i góra stożka są sześciokątami. Węzeł `Mesh.Triangles` służy do zwizualizowania rozkładu trójkątów siatki.


## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
