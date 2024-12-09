## Podrobnosti
Uzel `Mesh.Reduce` vytvoří novou síť se sníženým počtem trojúhelníků. Vstup `triangleCount` definuje cílový počet trojúhelníků výstupní sítě. Vemte na vědomí, že uzel `Mesh.Reduce` může výrazně změnit tvar sítě v případě extrémně agresivních cílových hodnot `triangleCount`. V následujícím příkladu se pomocí uzlu `Mesh.ImportFile` importuje síť, která je poté zmenšena uzlem `Mesh.Reduce` a přesunuta do jiné pozice pro lepší náhled a porovnání.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.Reduce_img.jpg)
