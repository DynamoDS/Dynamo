## Podrobnosti
Uzel Containment Test vrací booleovskou hodnotu podle toho, zda je daný bod obsažen uvnitř daného polygonu. Aby toto fungovalo, polygon musí být rovinný a nesmí protínat sám sebe. V níže uvedeném příkladu vytvoříme polygon pomocí řady bodů vytvořených pomocí uzlu By Cylindrical Coordinates. Ponecháním konstantní výšky a seřazením úhlů zajistíte rovinný polygon neprotínající sebe sama. Poté vytvoříme bod k testování a použijeme uzel ContainmentTest, abychom viděli, zda bod leží uvnitř nebo vně polygonu.
___
## Vzorový soubor

![ContainmentTest](./Autodesk.DesignScript.Geometry.Polygon.ContainmentTest_img.jpg)

