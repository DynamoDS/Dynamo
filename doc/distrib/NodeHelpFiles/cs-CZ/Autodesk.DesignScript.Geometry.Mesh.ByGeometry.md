## Podrobnosti
Uzel `Mesh.ByGeometry` přijímá jako vstup objekty geometrie aplikace Dynamo (povrchy nebo tělesa) a převádí je na síť. Body a křivky nemají žádné reprezentace sítě, takže nejsou platnými vstupy. Rozlišení sítě vytvořené při převodu je řízeno dvěma vstupy – `tolerance` a `maxGridLines`. `tolerance` nastavuje přijatelnou odchylku sítě od původní geometrie a je závislá na velikosti sítě. Pokud je hodnota `tolerance` nastavena na -1, aplikace Dynamo vybere rozumnou toleranci. Vstup `maxGridLines` nastavuje maximální počet čar osnovy ve směru U nebo V. Vyšší počet čar osnovy pomáhá zvýšit hladkost mozaiky.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
