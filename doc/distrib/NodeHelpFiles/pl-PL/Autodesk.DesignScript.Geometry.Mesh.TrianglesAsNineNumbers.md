## Informacje szczegółowe
Węzeł `Mesh.TrainglesAsNineNumbers` określa współrzędne X, Y i Z wierzchołków tworzących każdy trójkąt w podanej siatce, co daje w wyniku po dziewięć liczb na trójkąt. Ten węzeł może być przydatny do badania, rekonstruowania lub konwertowania pierwotnej siatki.

W poniższym przykładzie za pomocą węzłów `File Path` i `Mesh.ImportFile` zostaje zaimportowana siatka. Następnie za pomocą węzła `Mesh.TrianglesAsNineNumbers` zostają pobrane współrzędne wierzchołków każdego trójkąta. Ta lista zostaje następnie podzielona na trzy elementy za pomocą węzła `List.Chop` z pozycją danych wejściowych `lengths` ustawioną na wartość 3. Węzeł `List.GetItemAtIndex` pobiera współrzędne X, Y i Z i ponownie konstruuje wierzchołki przy użyciu węzła `Point.ByCoordinates`. Lista punktów zostaje dalej podzielona na zestawy po trzy (3 punkty dla każdego trójkąta) i zostaje użyta jako dane wejściowe dla węzła `Polygon.ByPoints`.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
