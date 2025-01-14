## Informacje szczegółowe
Ten węzeł zwraca nową wygładzoną siatkę za pomocą algorytmu wygładzania opartego na funkcji cotangens, który nie powoduje rozciągnięcia wierzchołków w stosunku do ich pierwotnego położenia i lepiej zachowuje elementy oraz krawędzie. Aby ustawić skalę przestrzenną wygładzania, w węźle należy wprowadzić wartość skalowania. Wartości skalowania mogą wynosić od 0,1 do 64,0. Wyższe wartości powodują bardziej zauważalny efekt wygładzania, co daje w wyniku siatkę wyglądającą na prostszą. Pomimo gładszego i prostszego wyglądu nowa siatka ma tę samą liczbę trójkątów, krawędzi i wierzchołków co początkowa.

W poniższym przykładzie węzeł `Mesh.ImportFile` służy do importowania obiektu. Następnie węzeł `Mesh.Smooth` wygładza obiekt ze skalą wygładzania równą 5. Obiekt jest następnie przenoszony do innego położenia za pomocą węzła `Mesh.Translate` w celu uzyskania lepszego podglądu, a węzeł `Mesh.TriangleCount` służy do śledzenia liczby trójkątów w starej i w nowej siatce.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Smooth_img.jpg)
