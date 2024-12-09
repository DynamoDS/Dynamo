## Informacje szczegółowe
Węzeł `Mesh.VertexIndicesByTri` zwraca spłaszczoną listę indeksów wierzchołków odpowiadających poszczególnym trójkątom siatki. Indeksy są uporządkowane w zestawach po trzy, a grupowanie indeksów można łatwo odtworzyć za pomocą węzła `List.Chop` z pozycją danych wejściowych `lengths` o wartości 3.

W poniższym przykładzie siatka `MeshToolkit.Mesh` z 20 trójkątami jest konwertowana na siatkę `Geometry.Mesh`. Za pomocą węzła `Mesh.VertexIndicesByTri` pobrana zostaje lista indeksów, która zostaje następnie podzielona na listy zestawów trzyelementowych za pomocą węzła `List.Chop`. Struktura listy zostaje odwrócona za pomocą węzła `List.Transpose` w celu uzyskania trzech list najwyższego poziomu, każda po 20 indeksów odpowiadających punktom A, B i C w poszczególnych trójkątach siatki. Węzeł `IndexGroup.ByIndices` tworzy grupy indeksów zawierające po trzy indeksy. Lista ustrukturyzowana `IndexGroups` i lista wierzchołków są następnie używane jako dane wejściowe węzła `Mesh.ByPointsFaceIndices` w celu uzyskania przekonwertowanej siatki.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
