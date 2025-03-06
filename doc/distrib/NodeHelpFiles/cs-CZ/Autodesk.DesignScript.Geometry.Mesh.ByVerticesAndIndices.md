## Podrobnosti
Uzel `Mesh.ByVerticesIndices` vezme seznam bodů `Points` představující položky vrcholů `vertices` trojúhelníků sítě a seznam indexů `indices` představující způsob sešití sítě dohromady a vytvoří novou síť. Vstup `vertices` by měl být plochý seznam jedinečných vrcholů v síti. Vstup `indices` by měl být plochý seznam celých čísel. Každá množina tří celých čísel označuje trojúhelník vsíti. Celá čísla určují index vrcholu v seznamu vrcholů. Vstup indexů by měl být indexován od 0, přičemž první bod seznamu vrcholů by měl mít index 0.

V následujícím příkladu se pomocí uzlu `Mesh.ByVerticesIndices` vytvoří síť ze seznamu devíti vrcholů `vertices` a seznamu 36 indexů `indices`, které určují kombinaci vrcholů pro každý z 12 trojúhelníků sítě.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)
