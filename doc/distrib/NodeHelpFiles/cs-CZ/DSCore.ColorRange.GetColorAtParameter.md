## Podrobnosti
Uzel GetColorAtParameter přijímá jako vstup 2D rozsah barev a vrátí seznam barev v určených parametrech UV v rozsahu od 0 do 1. V níže uvedeném příkladu nejprve vytvoříme 2D rozsah barev pomocí uzlu ByColorsAndParameters se vstupním seznamem barev a seznamem parametrů k nastavení rozsahu. Pomocí bloku kódu se vygeneruje rozsah čísel od 0 do 1, který se použije jako vstupy u a v v uzlu UV.ByCoordinates. Vázání tohoto uzlu je nastaveno na kartézský součin. Sada krychlí bude vytvořena podobným způsobem pomocí uzlu Point.ByCoordinates s vázáním pomocí kartézského součinu a výsledkem je pole krychlí. Poté je možné použít uzel Display.ByGeometryColor na pole krychlí a seznam barev, které vrátil uzel GetColorAtParameter.
___
## Vzorový soubor

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

