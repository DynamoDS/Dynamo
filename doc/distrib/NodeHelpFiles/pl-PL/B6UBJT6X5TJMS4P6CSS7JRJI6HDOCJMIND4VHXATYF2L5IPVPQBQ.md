<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
Węzeł `TSplineSurface.RemoveReflections` usuwa odbicia z pozycji danych wejściowych `tSplineSurface`. Usunięcie odbić nie modyfikuje kształtu, ale przerywa zależności między odbitymi częściami geometrii, umożliwiając edytowanie ich niezależnie.

W poniższym przykładzie najpierw zostaje utworzona powierzchnia T-splajn przez zastosowanie odbić osiowych i promieniowych. Powierzchnia zostaje następnie przekazana do węzła `TSplineSurface.RemoveReflections`, który usuwa odbicia. Aby zilustrować wpływ tego działania na późniejsze zmiany, jeden z wierzchołków zostaje przesunięty za pomocą węzła `TSplineSurface.MoveVertex`. Z powodu usunięcia z powierzchni odbić zmodyfikowany jest tylko jeden wierzchołek.

## Plik przykładowy

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
