## In-Depth
Węzeł `TSplineSurface.BuildPipes` generuje powierzchnię rurową T-splajn za pomocą sieci krzywych. Poszczególne rury są uważane za połączone, jeśli ich punkty końcowe mieszczą się w granicach maksymalnej tolerancji ustawionej za pomocą pozycji danych wejściowych `snappingTolerance`. Wynik tego węzła można dostosować za pomocą zestawu danych wejściowych, które umożliwiają ustawienie wartości dla wszystkich rur lub dla każdej osobno, jeśli pozycja danych wejściowych jest listą o długości równej liczbie rur. W ten sposób można używać następujących pozycji danych wejściowych: `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` i `endPositions`.

W poniższym przykładzie trzy krzywe połączone w punktach końcowych zostają przekazane jako dane wejściowe do węzła `TSplineSurface.BuildPipes`. Pozycja `defaultRadius` w tym przypadku jest pojedynczą wartością dla wszystkich trzech rur, która domyślnie definiuje promień rur, chyba że zostaną podane promienie początkowy i końcowy.
Następnie pozycja `segmentsCount` ustawia trzy różne wartości, po jednej dla każdej z rur. Ta pozycja danych wejściowych to lista trzech wartości, z których każda odpowiada rurze.

W przypadku ustawienia dla pozycji `autoHandleStart` i `autoHandleEnd` wartości False (Fałsz) dostępnych staje się więcej dopasowań. Umożliwia to sterowanie początkowym i końcowym obrotem każdej rury (pozycje danych wejściowych `startRotations` i `endRotations`) oraz promieniami na końcu i początku każdej rury przez określenie wartości `startRadii` i `endRadii`. Wreszcie pozycje `startPositions` i `endPositions` umożliwiają odsunięcie segmentów odpowiednio na początku lub końcu każdej krzywej. Ta pozycja danych wejściowych wymaga wartości odpowiadającej parametrowi krzywej wskazującemu, gdzie segmenty zaczynają się lub kończą (wartości od 0 do 1).

## Plik przykładowy
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
