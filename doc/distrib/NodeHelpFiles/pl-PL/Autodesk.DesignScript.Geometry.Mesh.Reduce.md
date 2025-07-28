## Informacje szczegółowe
Węzeł `Mesh.Reduce` tworzy nową siatkę ze zredukowaną liczbą trójkątów. Pozycja danych wejściowych `triangleCount` definiuje docelową liczbę trójkątów siatki wyjściowej. Należy pamiętać, że węzeł `Mesh.Reduce` może znacząco zmienić kształt siatki w przypadku bardzo agresywnych wartości docelowych `triangleCount`. W poniższym przykładzie węzeł `Mesh.ImportFile` służy do importowania siatki, która jest następnie zmniejszana przez węzeł `Mesh.Reduce` i przenoszona do innego położenia w celu uzyskania lepszego podglądu i porównania.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Reduce_img.jpg)
