## Podrobnosti
Uzel `Curve Mapper` využívá matematické křivky k přerozdělení bodů v definovaném rozsahu. Přerozdělení v tomto kontextu znamená znovupřiřazení souřadnic x k novým pozicím podél zadané křivky podle jejich souřadnic y. Tato technika je zvlášť užitečná v různých případech například návrh fasád, parametrické střešní konstrukce a další výpočty návrhu, kde jsou vyžadovány specifické vzory nebo rozložení.

Definujte meze pro souřadnice x nastavením minimálních a maximálních hodnot. Tyto meze určují hranice, v rámci kterých budou body přerozděleny. Můžete zadat buď jeden počet pro generování řady rovnoměrně rozdělených hodnot, nebo existující řadu hodnot, které budou rozděleny ve směru x v určeném rozsahu a poté mapovány na křivku.

Vyberte matematickou křivku z nabízených možností, mezi které patří lineární křivka, sinusoida, kosinusoida, Perlinův šum, Bezierova křivka, Gaussova křivka, parabola, odmocninová křivka a mocninová křivka. Pomocí interaktivních řídicích bodů můžete upravit tvar vybrané křivky a přizpůsobit ji vašim konkrétním potřebám.

Tvar křivky můžete uzamknout pomocí tlačítka zámku, čímž zabráníte dalším úpravám křivky. Kromě toho můžete obnovit výchozí stav tvaru pomocí tlačítka obnovení uvnitř uzlu. Pokud obdržíte jako výstup hodnotu NaN nebo Null, můžete si přečíst více [zde](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues) o důvodech této situace.

Chcete-li například přerozdělit 80 bodů podél sinusoidy v rozsahu 0 až 20, nastavte Min na 0, Max na 20 a Hodnoty na 80. Po výběru sinusoidy a úpravě jejího tvaru podle potřeby uzel `Curve Mapper` vypíše 80 bodů se souřadnicemi x, které sledují vzor sinusoidy podél osy y.

Chcete-li mapovat nerovnoměrně rozložené hodnoty podél Gaussovy křivky, nastavte minimální a maximální rozsah a zadejte řadu hodnot. Po výběru Gaussovy křivky a úpravě jejího tvaru podle potřeby uzel 'Curve Mapper' přerozdělí řadu hodnot podél souřadnic x pomocí zadaného rozsahu a namapuje hodnoty podél křivky křivky. Podrobné informace o tom, jak uzel funguje a jak nastavit vstupy, najdete v [tomto blog příspěvku](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) se zaměřením na mapovač křivek.




___
## Vzorový soubor

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
