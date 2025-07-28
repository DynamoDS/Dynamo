## Podrobnosti
Uzel 'Mesh.ByPointsIndices' přijímá seznam bodů, které představují vrcholy trojúhelníků sítě, a seznam indexů, které představují způsob, jakým je síť sešita, a vytvoří novou síť. Vstup 'points' by měl být plochý seznam jedinečných vrcholů v síti. Vstup 'indices' by měl být plochý seznam celých čísel. Každá sada tří celých čísel označuje trojúhelník v síti. Celá čísla určují index vrcholu v seznamu vrcholů. Vstup indexů by měl být indexovaný 0, přičemž první bod seznamu vrcholů by měl mít index 0.

V níže uvedeném příkladu je pomocí uzlu 'Mesh.ByPointsIndices' vytvořena síť pomocí seznamu devíti „bodů“ a seznamu 36 „indexů“, které určují kombinaci vrcholů pro každý z 12 trojúhelníků sítě.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
