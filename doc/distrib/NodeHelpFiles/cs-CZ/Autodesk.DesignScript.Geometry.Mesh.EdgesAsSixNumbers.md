## Podrobnosti
Uzel `Mesh.EdgesAsSixNumbers` určuje souřadnice X, Y a Z vrcholů tvořících každou jedinečnou hranu v zadané síti, což znamená šest čísel na hranu. Pomocí tohoto uzlu se můžete dotazovat nebo rekonstruovat síť, případně její hrany.

V následujícím příkladu se pomocí uzlu `Mesh.Cuboid` vytvoří kvádrová síť, která se pak použije jako vstup uzlu `Mesh.EdgesAsSixNumbers` k načtení seznamu hran vyjádřených jako šest čísel. Seznam je rozdělen do dalších seznamů po 6 položkách pomocí uzlu `List.Chop`, poté jsou pomocí uzlů `List.GetItemAtIndex` a `Point.ByCoordinates` rekonstruovány seznamy počátečních a koncových bodů každé hrany. Nakonec se pomocí uzlu `List.ByStartPointEndPoint` rekonstruují hrany sítě.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
