<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## Podrobnosti
Uzel `Solid.BySweep` vytvoří těleso tažením vstupní uzavřené křivky profilu podél zadané trajektorie.

V následujícím příkladu použijeme obdélník jako základní křivku profilu. Trajektorie se vytvoří pomocí funkce kosinus a posloupnosti úhlů, která zajistí, že se souřadnice x v sadě bodů budou lišit. Body se použijí jako vstup uzlu `NurbsCurve.ByPoints`. Poté vytvoříme těleso tažením obdélníku podél vytvořené křivky funkce kosinus.
___
## Vzorový soubor

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
