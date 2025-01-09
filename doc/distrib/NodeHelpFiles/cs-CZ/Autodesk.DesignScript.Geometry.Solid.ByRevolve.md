## Podrobnosti
Uzel `Solid.ByRevolve` vytvoří povrch otočením zadané křivky profilu kolem osy. Osa je definována bodem `axisOrigin` a vektorem `axisDirection`. Počáteční úhel určuje, kde má povrch začínat, je měřený ve stupních a vstup `sweepAngle` určuje, jak daleko kolem osy bude povrch pokračovat.

V následujícím příkladu použijeme křivku vytvořenou pomocí funkce kosinus jako křivku profilu a dva posuvníky k řízení vstupů `startAngle` a `sweepAngle`. Vstupy `axisOrigin` a `axisDirection` jsou v tomto příkladu ponechány na výchozích hodnotách globálního počátku a globální osy z.

___
## Vzorový soubor

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

