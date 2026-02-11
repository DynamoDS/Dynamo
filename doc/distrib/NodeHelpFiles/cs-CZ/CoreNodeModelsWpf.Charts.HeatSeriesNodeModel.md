## Podrobnosti

Vykreslení tepelné řady vytvoří graf, ve kterém jsou datové body znázorněny jako obdélníky v různých barvách rozsahu barev.

Každému sloupci a řádku přiřaďte popisky zadáním seznamu řetězcových popisků do vstupních polí popisků osy x a popisků osy y. Počet popisků osy x a popisků osy y nemusí být shodný.

Pro každý obdélník definujte hodnotu ve vstupním poli hodnot. Počet dílčích seznamů musí odpovídat počtu řetězcových hodnot ve vstupním poli popisků osy x, protože představuje počet sloupců. Hodnoty uvnitř každého dílčího seznamu představují počet obdélníků v každém sloupci. Například 4 dílčí seznamy odpovídají 4 sloupcům a pokud každý dílčí seznam má 5 hodnot, sloupce mají každý 5 obdélníků.

V rámci dalšího příkladu zadejte k vytvoření osnovy s 5 řádky a 5 sloupci 5 řetězcových hodnot do vstupního pole popisků osy x a také do vstupního pole popisků osy y. Hodnoty popisků osy x se zobrazí pod grafem podél osy x a hodnoty popisků osy y se zobrazí vlevo od grafu podél osy y.

Do vstupního pole hodnot zadejte seznam seznamů, přičemž každý dílčí seznam bude obsahovat 5 hodnot. Hodnoty jsou vykreslovány po sloupcích zleva doprava a zdola nahoru, takže první hodnota v prvním dílčím seznamu je dolní obdélník v levém sloupci, druhá hodnota je obdélník nad tímto a tak dále. Každý dílčí seznam představuje sloupec ve vykreslení.

Za účelem rozlišení datových bodů můžete přiřadit rozsah barev zadáním seznamu hodnot barev do vstupního pole barev. Nejnižší hodnota v grafu bude rovna první barvě a nejvyšší hodnota bude rovna poslední barvě, s dalšími hodnotami znázorněnými podle gradientu. Pokud není přiřazen žádný rozsah barev, datovým bodům bude přiřazena náhodná barva od nejsvětlejší po nejtmavší odstín.

Nejlepších výsledků dosáhnete, pokud použijete jednu nebo dvě barvy. Vzorový soubor obsahuje klasický příklad dvou barev, modré a červené. Pokud se použijí jako vstupy barev, Vykreslení tepelné řady automaticky vytvoří gradient mezi těmito barvami, přičemž nízké hodnoty budou znázorněny v odstínech modré a vysoké hodnoty v odstínech červené.

___
## Vzorový soubor

![Heat Series Plot](./CoreNodeModelsWpf.Charts.HeatSeriesNodeModel_img.jpg)

