## Informacje szczegółowe
Węzeł `Mesh.ByPointsIndices` pobiera listę punktów (`Points`) reprezentujących wierzchołki `vertices` trójkątów siatki oraz listę indeksów `indices` reprezentującą sposób zszywania siatki i tworzy nową siatkę. Pozycja danych wejściowych `points` powinna być płaską listą unikatowych wierzchołków w siatce. Pozycja danych wejściowych `indices` powinna być płaską listą liczb całkowitych. Każdy zestaw trzech liczb całkowitych wyznacza trójkąt w siatce. Liczby całkowite określają indeks wierzchołka na liście wierzchołków. Dane wejściowe indeksów powinny być indeksowane od 0 i pierwszy punkt listy wierzchołków powinien mieć indeks 0.

W poniższym przykładzie węzeł `Mesh.ByPointsIndices` tworzy siatkę przy użyciu listy dziewięciu punktów, `points`, i listy 36 indeksów, `indices`, określających kombinację wierzchołków dla każdego z 12 trójkątów siatki.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
