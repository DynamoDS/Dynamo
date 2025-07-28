## Podrobnosti
Uzel ByColorsAndParameters vytvoří 2D rozsah barev ze seznamu vstupních barev a odpovídajícího seznamu určených parametrů UV v rozsahu od 0 do 1. V níže uvedeném příkladu vytvoříme pomocí bloku kódu tři různé barvy (v tomto případě jednoduše zelenou, červenou a modrou) a sloučíme je do seznamu. Pomocí samostatného bloku kódu vytvoříme tři parametry UV, pro každou barvu jeden. Tyto dva seznamy se použijí jako vstupy uzlu ByColorsAndParameters. K vizualizaci 2D barevného rozsahu na sadě krychlí použijeme následný uzel GetColorAtParameter společně s uzlem Display.ByGeometryColor.
___
## Vzorový soubor

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

