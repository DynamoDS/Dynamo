## Informacje szczegółowe
Za pomocą operacji `Mesh.MakeHollow` można wydrążyć obiekt w postaci siatki w ramach przygotowań do drukowania 3D. Wydrążenie siatki może znacznie zmniejszyć ilość materiału wymaganego do drukowania, czas drukowania i koszty. Pozycja danych wejściowych `wallThickness` definiuje grubość ścian obiektu siatki. Opcjonalnie węzeł `Mesh.MakeHollow` może generować otwory ewakuacyjne, aby można było usunąć nadmiar materiału podczas procesu drukowania. Rozmiarami i liczbą otworów sterują dane wejściowe `holeCount` i `holeRadius`. Wreszcie dane wejściowe `meshResolution` i `solidResolution` wpływają na rozdzielczość wyniku siatki. Wyższa wartość rozdzielczości `meshResolution` zwiększa dokładność, w kierunku której wewnętrzna część siatki odsuwa pierwotną siatkę, ale powoduje to utworzenie większej liczby trójkątów. Wyższa wartość `solidResolution` zwiększa stopień zachowania drobniejszych szczegółów pierwotnej siatki w wewnętrznej części wydrążonej siatki.
W poniższym przykładzie węzeł `Mesh.MakeHollow` jest stosowany do siatki w kształcie stożka. W jego podstawie dodano pięć otworów ewakuacyjnych.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
