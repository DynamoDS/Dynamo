## Informacje szczegółowe
Węzeł `Mesh.CloseCracks` zamyka pęknięcia w siatce przez usunięcie wewnętrznych obwiedni z obiektu siatki. Wewnętrzne obwiednie mogą powstawać w naturalny sposób w wyniku operacji modelowania siatki. W razie usuwania zdegenerowanych krawędzi w ramach tej operacji mogą zostać usunięte trójkąty. W poniższym przykładzie do zaimportowanej siatki jest stosowany węzeł `Mesh.CloseCracks`. Węzeł `Mesh.VertexNormals` służy do wizualizacji nakładających się wierzchołków. Po przepuszczeniu pierwotnej siatki przez węzeł Mesh.CloseCracks liczba krawędzi jest zmniejszona, co jest również widoczne wskutek porównania liczby krawędzi przy użyciu węzła `Mesh.EdgeCount`.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
