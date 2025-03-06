## In profondità
PolySurface.ByJoinedSurfaces utilizza un elenco di superfici come input e restituirà un singolo solido definito dalle superfici. Le superfici devono definire una superficie chiusa. Nell'esempio seguente, si inizia con un cerchio come geometria di base. Il cerchio viene coperto per creare una superficie e tale superficie viene traslata nella direzione z. Si estrude quindi il cerchio per produrre i lati. Viene utilizzato List.Create per creare un elenco costituito da superfici di base, laterali e superiori e poi si utilizza ByJoinedSurfaces per trasformare l'elenco in un singolo solido chiuso.
___
## File di esempio

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

