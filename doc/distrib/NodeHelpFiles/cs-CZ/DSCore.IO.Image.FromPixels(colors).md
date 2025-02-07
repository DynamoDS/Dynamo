## Podrobnosti
Uzel From Pixels vytvoří objekt obrázku ze vstupního dvourozměrného pole barev. V níže uvedeném příkladu nejprve vygenerujeme rozsah čísel od 0 do 255 pomocí bloku kódu. K tvorbě barev z tohoto rozsahu se použije uzel Color.ByARGB a vázání tohoto uzlu se nastaví na kartézský součin, čímž vznikne dvourozměrné pole. Poté pomocí uzlu Image.FromPixels vytvoříme obrázek. Náhled vytvořeného obrázku je možné zobrazit pomocí uzlu Watch Image.
___
## Vzorový soubor

![FromPixels (colors)](./DSCore.IO.Image.FromPixels(colors)_img.jpg)

