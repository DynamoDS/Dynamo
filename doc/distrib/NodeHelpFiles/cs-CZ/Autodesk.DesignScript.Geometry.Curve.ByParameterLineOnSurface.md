## Podrobnosti
Uzel Curve by Parameter Line On Surface vytvoří úsečku podél povrchu mezi dvěma vstupními souřadnicemi UV. V níže uvedeném příkladu nejprve vytvoříme osnovu bodů a náhodně je přesuneme ve směru Z. Pomocí těchto bodů a uzlu NurbsSurface.ByPoints se vytvoří povrch. Tento povrch se použije jako objekt baseSurface uzlu ByParameterLineOnSurface. Pomocí sady číselných posuvníků upravíme vstupy U a V dvou uzlů UV.ByCoordinates, pomocí nichž se poté určí počáteční a koncový bod úsečky na povrchu.
___
## Vzorový soubor

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

