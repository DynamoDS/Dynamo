## Informacje szczegółowe
Węzeł `Mesh.ExtrudePolyCurve` wyciąga podaną krzywą złożoną `polycurve` na określoną odległość ustawioną za pomocą wartości wejściowej `height` i w kierunku określonego wektora. Otwarte krzywe polycurve są zamykane przez połączenie pierwszego punktu z ostatnim. Jeśli początkowa krzywa `polycurve` jest płaska i nie przecina się sama ze sobą, wynikowa siatka może zostać zamknięta w celu utworzenia siatki bryły.
W poniższym przykładzie za pomocą węzła `Mesh.ExtrudePolyCurve` tworzona jest siatka zamknięta na podstawie krzywej zamkniętej polycurve.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.ExtrudePolyCurve_img.jpg)
