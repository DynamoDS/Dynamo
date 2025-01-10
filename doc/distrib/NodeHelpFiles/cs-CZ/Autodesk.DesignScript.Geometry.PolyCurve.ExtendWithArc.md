## Podrobnosti
Uzel Extend With Arc přidá kruhový oblouk na začátek nebo konec vstupního objektu PolyCurve a vrací jeden kombinovaný objekt PolyCurve. Vstup radius určí poloměr kružnice, zatímco vstup length určí vzdálenost podél kružnice určující oblouk. Celková délka musí být menší nebo rovna délce celé kružnice s daným poloměrem. Vygenerovaný oblouk bude tečný ke konci vstupního objektu PolyCurve. Booleovský vstup endOrStart řídí, na kterém konci objektu PolyCurve se oblouk vytvoří. Hodnota 'true' způsobí, že se oblouk vytvoří na konci objektu PolyCurve, zatímco hodnota 'false' vytvoří oblouk na začátku objektu PolyCurve. V následujícím příkladu nejprve vygenerujeme objekt PolyCurve pomocí sady náhodných bodů a uzlu PolyCurve By Points. Poté použijeme dva posuvníky a booleovský přepínač k nastavení parametrů uzlu ExtendWithArc.
___
## Vzorový soubor

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

