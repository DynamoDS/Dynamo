## Podrobnosti
Uzel `Mesh.Sphere` vytvoří síťovou kouli vystředěnou ve vstupním bodě `origin` s daným poloměrem `radius` a počtem dělení `divisions`. Booleovský vstup `icosphere` slouží k přepínání mezi typy kulových sítí `icosphere` a `UV-Sphere`. Typ icosphere pokrývá kouli více pravidelnými trojúhelníky než síť UV a obvykle poskytuje lepší výsledky při následných modelovacích operacích. U sítě UV jsou póly zarovnány s osou koule a vrstvy trojúhelníků se generují podélně kolem osy.

V případě sítě icosphere by počet trojúhelníků kolem osy koule mohl být stejně nízký, jako je zadaný počet dělení, a maximálně dvojnásobný od tohoto počtu. Dělení sítě `UV-sphere` určuje počet vrstev trojúhelníků generovaných podélně kolem koule. Pokud je vstup `divisions` nastaven na nulu, uzel vrátí UV kouli s výchozím počtem 32 dělení u obou typů sítě.

V níže uvedeném příkladu se uzel `Mesh.Sphere` používá k vytvoření dvou koulí se stejným poloměrem a děleními, ale pomocí různých metod. Pokud je vstup `icosphere` nastaven na hodnotu `True`, uzel `Mesh.Sphere` vrátí síť typu `icosphere`. Případně, pokud je vstup `icosphere` nastaven na hodnotu `False`, uzel `Mesh.Sphere` vrátí síť typu `UV-sphere`.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
