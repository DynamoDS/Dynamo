## Podrobnosti
Uzel `Mesh.VertexIndicesByTri` vrací zploštělý seznam indexů vrcholů odpovídajících jednotlivým trojúhelníkům sítě. Indexy jsou seřazeny po třech a seskupení indexů lze snadno rekonstruovat pomocí uzlu `List.Chop` s hodnotou 3 na vstupu `lengths`.

V následujícím příkladu je objekt `MeshToolkit.Mesh` s 20 trojúhelníky převeden na objekt `Geometry.Mesh`. Pomocí uzlu `Mesh.VertexIndicesByTri` se získá seznam indexů, který je poté rozdělen do seznamů po trojicích pomocí uzlu `List.Chop`. Struktura seznamu je převrácena pomocí uzlu `List.Transpose` k získání tří seznamů nejvyšší úrovně s 20 indexy odpovídajícími bodům A, B a C v každém trojúhelníku sítě. Uzel `IndexGroup.ByIndices` slouží k vytvoření skupin indexů po třech. Strukturovaný seznam `IndexGroups` a seznam vrcholů se poté použije jako vstup pro uzel `Mesh.ByPointsFaceIndices` k získání převedené sítě.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
