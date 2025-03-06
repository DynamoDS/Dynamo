## Podrobnosti
Uzel From Pixels se šířkou a výškou vytvoří obrázek ze vstupního plochého seznamu barev, kde se každá barva stane jedním pixelem. Šířka vynásobená výškou by měla být rovna celkovému počtu barev. V níže uvedeném příkladu nejprve vytvoříme seznam barev pomocí uzlu ByARGB. Blok kódu vytvoří rozsah hodnot od 0 do 255, což po připojení ke vstupům r a g vytvoří řadu barev od černé po žlutou. Vytvoříme obrázek o šířce 8. K určení výšky obrázku se použijí uzly Count a Division. Náhled vytvořeného obrázku je možné zobrazit pomocí uzlu Watch Image.
___
## Vzorový soubor

![FromPixels (colors, width, height)](./DSCore.IO.Image.FromPixels(colors,%20width,%20height)_img.jpg)

