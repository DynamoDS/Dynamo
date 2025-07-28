## Informacje szczegółowe
W poniższym przykładzie prosta powierzchnia prostopadłościanu T-splajn zostaje przekształcona w siatkę za pomocą węzła `TSplineSurface.ToMesh`. Pozycja danych wejściowych `minSegments` definiuje minimalną liczbę segmentów dla powierzchni w każdym kierunku i jest ważna przy sterowaniu definicją siatki. Pozycja danych wejściowych `tolerance` poprawia niedokładności przez dodanie większej liczby pozycji wierzchołków w celu dopasowania do powierzchni pierwotnej w ramach danej tolerancji. Wynikiem jest siatka, której definicja jest wyświetlana za pomocą węzła `Mesh.VertexPositions`.
Siatka wyjściowa może zawierać trójkąty i czworoboki, o czym należy pamiętać w przypadku używania węzłów MeshToolkit.
___
## Plik przykładowy

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
