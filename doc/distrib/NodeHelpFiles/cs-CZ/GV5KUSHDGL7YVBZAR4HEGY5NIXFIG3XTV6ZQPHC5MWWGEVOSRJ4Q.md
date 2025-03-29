## Podrobnosti
Uzel `Curve Mapper` využívá matematické křivky k přerozdělení bodů v definovaném rozsahu. Přerozdělení v tomto kontextu znamená znovupřiřazení souřadnic x k novým pozicím podél zadané křivky podle jejich souřadnic y. Tato technika je zvlášť užitečná v různých případech například návrh fasád, parametrické střešní konstrukce a další výpočty návrhu, kde jsou vyžadovány specifické vzory nebo rozložení.

Definujte meze pro souřadnice x a y nastavením minimálních a maximálních hodnot. Tyto meze určují hranice, v rámci kterých budou body přerozděleny. Dále vyberte matematickou křivku z nabízených možností, mezi které patří lineární křivka, sinusoida, kosinusoida, Perlinův šum, Bezierova křivka, Gaussova křivka, parabola, odmocninová křivka a mocninová křivka. Pomocí interaktivních řídicích bodů můžete upravit tvar vybrané křivky a přizpůsobit ji vašim konkrétním potřebám.

Tvar křivky můžete uzamknout pomocí tlačítka zámku, čímž zabráníte dalším úpravám křivky. Kromě toho můžete obnovit výchozí stav tvaru pomocí tlačítka obnovení uvnitř uzlu.

Určete počet bodů, které mají být přerozděleny, nastavením vstupu Count. Uzel vypočítá nové souřadnice x pro zadaný počet bodů podle vybrané křivky a definovaných mezí. Body se přerozdělí tak, že jejich souřadnice x sledují tvar křivky podél osy y.

Chcete-li například přerozdělit 80 bodů podél sinusoidy, nastavte možnost Min X na hodnotu 0, Max X na hodnotu 20, Min Y na hodnotu 0 a Max Y na hodnotu 10. Po výběru sinusoidy a úpravě jejího tvaru podle potřeby uzel `Curve Mapper` vypíše 80 bodů se souřadnicemi x, které sledují vzor sinusoidy podél osy y od 0 do 10.




___
## Vzorový soubor

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
