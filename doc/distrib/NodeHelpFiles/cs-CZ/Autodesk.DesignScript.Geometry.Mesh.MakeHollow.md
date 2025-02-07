## Podrobnosti
Operaci `Mesh.MakeHollow` lze použít k vyhloubení objektu sítě v rámci přípravy na 3D tisk. Vyhloubení sítě může výrazně snížit množství potřebného tiskového materiálu, snížit náklady a zkrátit dobu tisku. Vstup `wallThickness` definuje tloušťku stěn objektu sítě. Volitelně může uzel `Mesh.MakeHollow` generovat únikové otvory, které odstraňují přebytečný materiál během procesu tisku. Velikost a počet otvorů jsou řízeny vstupy `holeCount` a `holeRadius`. Nakonec vstupy `meshResolution` a `solidResolution` ovlivňují rozlišení výsledku sítě. Vyšší hodnota `meshResolution` zlepšuje přesnost, s jakou vnitřní část sítě odsadí původní síť, ale vznikne více trojúhelníků. Vyšší hodnota `solidResolution` zlepšuje rozsah, ve kterém jsou zachovány jemnější detaily původní sítě ve vnitřní části duté sítě.
V následujícím příkladu se metoda `Mesh.MakeHollow` použije na síť ve tvaru kužele. Na jeho základnu je přidáno pět únikových otvorů.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
