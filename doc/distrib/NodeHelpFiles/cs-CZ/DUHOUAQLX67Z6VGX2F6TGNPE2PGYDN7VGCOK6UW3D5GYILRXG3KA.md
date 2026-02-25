<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## Podrobnosti
Uzel `Curve.SweepAsSurface` vytvoří povrch tažením vstupní křivky podél určené trajektorie. V následujícím příkladu vytvoříme křivku k tažení pomocí bloku kódu, který vytvoří tři body uzlu `Arc.ByThreePoints`. Křivka trajektorie se vytvoří jako jednoduchá čára podél osy x. Uzel `Curve.SweepAsSurface` přesune křivku profilu podél křivky trajektorie, čímž vytvoří povrch. Parametr `cutEndOff` je booleovská hodnota, která určuje zakončení taženého povrchu. Pokud je hodnota nastavena na `true`, konce povrchu jsou oříznuty kolmo (normálně) ke křivce trajektorie, čímž se vytvoří čistá, plochá ukončení. Pokud je hodnota nastavena na `false`(výchozí nastavení), konce povrchu sledují přirozený tvar křivky profilu bez ořezávání, což může mít za následek šikmé nebo nerovnoměrné konce v závislosti na zakřivení cesty.
___
## Vzorový soubor

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

