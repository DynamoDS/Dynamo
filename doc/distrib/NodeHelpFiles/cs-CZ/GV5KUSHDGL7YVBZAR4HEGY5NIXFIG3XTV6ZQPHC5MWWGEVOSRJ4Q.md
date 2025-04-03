## Podrobnosti
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Definujte meze pro souřadnice x a y nastavením minimálních a maximálních hodnot. Tyto meze určují hranice, v rámci kterých budou body přerozděleny. Dále vyberte matematickou křivku z nabízených možností, mezi které patří lineární křivka, sinusoida, kosinusoida, Perlinův šum, Bezierova křivka, Gaussova křivka, parabola, odmocninová křivka a mocninová křivka. Pomocí interaktivních řídicích bodů můžete upravit tvar vybrané křivky a přizpůsobit ji vašim konkrétním potřebám.

Tvar křivky můžete uzamknout pomocí tlačítka zámku, čímž zabráníte dalším úpravám křivky. Kromě toho můžete obnovit výchozí stav tvaru pomocí tlačítka obnovení uvnitř uzlu.

Určete počet bodů, které mají být přerozděleny, nastavením vstupu Count. Uzel vypočítá nové souřadnice x pro zadaný počet bodů podle vybrané křivky a definovaných mezí. Body se přerozdělí tak, že jejich souřadnice x sledují tvar křivky podél osy y.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## Vzorový soubor


