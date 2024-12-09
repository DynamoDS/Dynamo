<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## Podrobnosti
Režim kvádru a režim vyhlazení jsou dva způsoby zobrazení povrchu T-Spline. Režim vyhlazení je skutečný tvar povrchu T-Spline a je užitečný pro náhled estetiky a rozměrů modelu. Režim kvádru může naopak vrhat vhled na povrchovou strukturu a usnadnit její pochopení a jedná se také o rychlejší možnost zobrazení náhledu velké nebo složité geometrie. Uzel `TSplineSurface.EnableSmoothMode` umožňuje přepínat mezi těmito dvěma stavy náhledu v různých fázích vývoje geometrie.

V níže uvedeném příkladu se provede operace zkosení na povrchu kvádru T-Spline. Výsledek je nejprve vizualizován v režimu kvádru (vstup `inSmoothMode` povrchu kvádru je nastaven na hodnotu false), aby bylo možné lépe pochopit strukturu tvaru. Poté se aktivuje režim vyhlazení pomocí uzlu `TSplineSurface.EnableSmoothMode` a výsledek se posune doprava, aby bylo možné zobrazit náhled obou režimů najednou.
___
## Vzorový soubor

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
