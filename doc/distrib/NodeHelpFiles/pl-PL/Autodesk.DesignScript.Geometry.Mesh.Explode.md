## Informacje szczegółowe
Węzeł `Mesh.Explode` pobiera pojedynczą siatkę i zwraca listę powierzchni siatki jako niezależne siatki.

W poniższym przykładzie przedstawiono kopułę w formie siatki, która została rozbita za pomocą węzła `Mesh.Explode`, a następnie odsunięcie każdej powierzchni w kierunku wektora normalnego tej powierzchni. Można to zrealizować za pomocą węzłów `Mesh.TriangleNormals` i `Mesh.Translate`. Mimo że w tym przykładzie powierzchnie siatki wydają się być czworokątami, w rzeczywistości są trójkątami o identycznych wektorach normalnych.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
