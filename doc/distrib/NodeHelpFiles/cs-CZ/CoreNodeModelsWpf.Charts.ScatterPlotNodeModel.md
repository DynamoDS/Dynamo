## Podrobnosti

Uzel Bodový graf vytvoří graf s body vykreslenými podle hodnot X a Y a v barevném rozlišení podle skupin.
Opatřete skupiny popisky nebo změňte počet skupin vložením seznamu řetězcových hodnot do vstupního pole popisků. Každý popisek vytvoří odpovídající barevně rozlišenou skupinu. Pokud zadáte pouze jednu řetězcovou hodnotu, všechny body budou mít stejnou barvu a budou mít sdílený popisek.

Chcete-li určit umístění každého bodu, použijte seznam seznamů obsahující hodnoty typu double ve vstupních polích hodnot x a y. Ve vstupních polích hodnot X a Y musí být stejný počet hodnot. Počet dílčích seznamů musí také odpovídat počtu řetězcových hodnot ve vstupním poli popisků.

Chcete-li přiřadit barvu ke každé skupině, vložte seznam barev do vstupního pole barev. Při přiřazování vlastních barev musí počet barev odpovídat počtu řetězcových hodnot ve vstupním poli popisků. Pokud nejsou přiřazeny žádné barvy, použijí se náhodné barvy.

___
## Vzorový soubor

![Scatter Plot](./CoreNodeModelsWpf.Charts.ScatterPlotNodeModel_img.jpg)

