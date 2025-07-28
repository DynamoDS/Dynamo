## Podrobnosti
Uzel Curve At Index vrátí segment křivky na vstupním indexu daného objektu polycurve. Pokud je počet křivek v objektu polycurve menší než zadaný index,uzel CurveAtIndex vrátí hodnotu null. Vstup endOrStart přijímá booleovskou hodnotu true nebo false. Pokud je tato hodnota true, uzel CurveAtIndex začne počítat od prvního segmentu objektu PolyCurve. Pokud je hodnota false, uzel bude počítat zpětně od posledního segmentu. V níže uvedeném příkladu vygenerujeme sadu náhodných bodů a poté pomocí uzlu PolyCurve By Points vytvoříme otevřený objekt PolyCurve. Poté je možné pomocí uzlu CurveAtIndex extrahovat konkrétní segmenty z objektu PolyCurve.
___
## Vzorový soubor

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

