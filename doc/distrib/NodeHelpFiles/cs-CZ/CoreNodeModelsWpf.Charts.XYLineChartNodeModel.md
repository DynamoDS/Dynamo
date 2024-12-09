## Podrobnosti

Graf XY čar vytvoří graf s jednou nebo více čarami vykreslenými podle jejich hodnot X a Y. Opatřete vaše čáry popisky nebo změňte počet čar zadáním seznamu řetězcových popisků do vstupního pole popisků. Každý popisek vytvoří novou barevně rozlišenou čáru. Pokud zadáte pouze jednu řetězcovou hodnotu, vytvoří se pouze jedna čára.

Chcete-li určit umístění každého bodu podél každé čáry, použijte seznam seznamů obsahující hodnoty typu double ve vstupních polích hodnot x a y. Ve vstupních polích hodnot X a Y musí být stejný počet hodnot. Počet dílčích seznamů musí také odpovídat počtu řetězcových hodnot ve vstupním poli popisků.
Pokud například chcete vytvořit 3 čáry, každou s 5 body, zadejte seznam 3 řetězcových hodnot do vstupního pole popisků, abyste pojmenovali každou čáru, a dále zadejte 3 dílčí seznamy, každý s 5 hodnotami typu double pro hodnoty X a Y.

Chcete-li každé čáře přiřadit barvu, vložte seznam barev do vstupního pole barev. Při přiřazování vlastních barev musí počet barev odpovídat počtu řetězcových hodnot ve vstupním poli popisků. Pokud nejsou přiřazeny žádné barvy, použijí se náhodné barvy.

___
## Vzorový soubor

![XY Line Plot](./CoreNodeModelsWpf.Charts.XYLineChartNodeModel_img.jpg)

