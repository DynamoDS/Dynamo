## Informacje szczegółowe
Ten węzeł zlicza krawędzie w dostarczonej siatce. Jeśli siatka składa się z trójkątów, co jest prawdą w przypadku wszystkich siatek w zestawie `MeshToolkit`, węzeł `Mesh.EdgeCount` zwraca tylko unikatowe krawędzie. W rezultacie należy się spodziewać, że liczba krawędzi nie będzie trzykrotnie większa od liczby trójkątów w siatce. Na podstawie tego założenia można sprawdzić, czy siatka nie zawiera żadnych niezłączonych powierzchni (może to wystąpić w zaimportowanych siatkach).

W poniższym przykładzie za pomocą węzłów `Mesh.Cone` i `Number.Slider` tworzony jest stożek, który jest następnie używany jako dane wejściowe dla zliczania krawędzi. Za pomocą węzłów `Mesh.Edges` i `Mesh.Triangles` można wyświetlić podgląd struktury i siatki (węzeł `Mesh.Edges` wykazuje lepszą wydajność w przypadku złożonych i „ciężkich” siatek).

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
