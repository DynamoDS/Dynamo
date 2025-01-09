## Podrobnosti
Vrací novou síť s opravenými následujícími vadami:
- Malé komponenty: pokud síť obsahuje velmi malé (vzhledem k celkové velikosti sítě) odpojené segmenty, budou vyřazeny.
- Díry: díry v síti jsou vyplněny.
- Nerozložené oblasti: pokud je vrchol připojen k více než dvěma *okrajovým* hranám nebo je hrana připojena k více než dvěma trojúhelníkům, pak je vrchol/hrana nerozložená. Sada nástrojů pro sítě bude odebírat geometrii, dokud nebude síť rozložená.
Tato metoda se snaží zachovat co nejvíce z původní sítě, na rozdíl od metody MakeWatertight, která síť převzorkuje.

V následujícím příkladu se u importované sítě použije uzel `Mesh.Repair` k vyplnění díry kolem ucha králíčka.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.Repair_img.jpg)
