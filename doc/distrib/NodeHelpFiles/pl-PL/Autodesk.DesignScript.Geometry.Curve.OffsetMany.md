## Informacje szczegółowe
Węzeł `Curve.OffsetMany` tworzy jedną lub więcej krzywych przez odsunięcie krzywej płaskiej o daną odległość w płaszczyźnie zdefiniowanej przez wektor normalny płaszczyzny. Jeśli między krzywymi składowymi odsunięcia występują przerwy, są one wypełniane przez wydłużenie krzywych odsunięcia.

Pozycja danych wejściowych `planeNormal` jest domyślnie wektorem normalnym płaszczyzny zawierającej krzywą. Można jednak jawnie podać wektor normalny równoległy do oryginalnego wektora normalnego krzywej, aby dokładniej sterować kierunkiem odsunięcia.

Jeśli na przykład wymagany jest stały kierunek odsunięcia dla wielu krzywych współdzielących tę samą płaszczyznę, można za pomocą pozycji danych wejściowych `planeNormal` nadpisać wektory normalne poszczególnych krzywych i wymusić odsunięcie wszystkich krzywych w tym samym kierunku. Odwrócenie wektora normalnego spowoduje odwrócenie kierunku odsunięcia.

W poniższym przykładzie krzywa PolyCurve zostaje odsunięta o ujemną odległość odsunięcia, która jest stosowana w kierunku przeciwnym do kierunku iloczynu wektorowego stycznej krzywej i wektora normalnego płaszczyzny.
___
## Plik przykładowy

![Curve.OffsetMany](./Autodesk.DesignScript.Geometry.Curve.OffsetMany_img.jpg)
