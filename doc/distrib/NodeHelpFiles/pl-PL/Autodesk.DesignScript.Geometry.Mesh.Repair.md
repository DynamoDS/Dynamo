## Informacje szczegółowe
Zwraca nową siatkę z naprawionymi następującymi wadami:
- Małe komponenty: jeśli siatka zawiera bardzo małe (w stosunku do ogólnego rozmiaru siatki) rozłączone segmenty, zostaną one odrzucone.
- Otwory: otwory w siatce są wypełniane.
- Obszary nierozmaitościowe: jeśli wierzchołek jest połączony z więcej niż dwiema krawędziami *granicznymi* lub krawędź jest połączona z więcej niż dwoma trójkątami, wierzchołek/krawędź jest nierozmaitościowa. Zestaw narzędzi siatki będzie usuwać geometrię, dopóki siatka nie stanie się rozmaitościowa.
Ta metoda stara się zachować jak największą część pierwotnej siatki — w przeciwieństwie do metody MakeWatertight, która polega na ponownym próbkowaniu siatki.

W poniższym przykładzie węzeł `Mesh.Repair` jest stosowany do zaimportowanej siatki w celu wypełnienia otworu wokół ucha królika.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Repair_img.jpg)
