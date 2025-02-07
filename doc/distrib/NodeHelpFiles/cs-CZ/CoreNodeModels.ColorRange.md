## Podrobnosti
Uzel Color Range vytvoří gradient mezi sadou vstupních barev a umožní výběr barev tohoto gradientu prostřednictvím seznamu vstupních hodnot. První vstup, colors, je seznam barev, které se mají použít v gradientu. Druhý vstup, indices, určí relativní umístění vstupních barev v gradientu. Tento seznam by měl odpovídat seznamu barev, přičemž každá hodnota je v rozsahu od 0 do 1. Přesná hodnota není důležitá, pouze relativní pozice hodnot. Barva odpovídající nejnižší hodnotě bude v levé části gradientu a barva odpovídající nejvyšší hodnotě bude v pravé části gradientu. Vstup konečných hodnot umožní uživateli vybrat body v rámci gradientu v rozsahu od 0 do 1 k umístění na výstup. V níže uvedeném příkladu nejprve vytvoříme dvě barvy: červenou a modrou. Pořadí těchto barev v gradientu je určeno seznamem, který vytvoříme pomocí bloku kódu. Pomocí třetího bloku kódu se vytvoří rozsah čísel mezi 0 a 1, který určí výstupní barvy gradientu. Vygeneruje se sada krychlí podél osy x a tyto krychle jsou nabarveny podle gradientu pomocí uzlu Display.ByGeometryColor.
___
## Vzorový soubor

![Color Range](./CoreNodeModels.ColorRange_img.jpg)

