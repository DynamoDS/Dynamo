## Podrobnosti
Uzel Patch se pokusí vytvořit povrch pomocí vstupní křivky jako hranice. Vstupní křivka musí být uzavřená. V níže uvedeném příkladu nejprve pomocí uzlu Point.ByCylindricalCoordinates vytvoříme sadu bodů v nastavených intervalech v kružnici, ale s náhodnými výškami a poloměry. Poté pomocí objektu NurbsCurve.ByPoints vytvoříme uzavřenou křivku podle těchto bodů. Pomocí uzlu Patch se vytvoří povrch z uzavřené křivky hranice. Všimněte si, že kvůli tvorbě bodů v náhodných poloměrech a výškách ne všechna uspořádání spějí k vytvoření křivky, kterou je možné záplatovat.
___
## Vzorový soubor

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

