## Podrobnosti
Uzel Point At Chord Length vrátí bod, který se nachází v dané délce tětivy od počátečního určeného parametru. V níže uvedeném příkladu nejprve vytvoříme křivku Nurbs pomocí uzlu ByControlPoints, přičemž jako vstup se použije sada náhodně generovaných bodů. K řízení délky tětivy přímé čáry, ve které se má hledat bod, se použije číselný posuvník. Pomocí druhého číselného posuvníku nastaveného na rozsah 0 až 1 je možné řídit počáteční bod podél křivky, od kterého se bude měřit délka tětivy. Nakonec se pomocí booleovského přepínače určí směr od kterého se bude měřit délka tětivy.
___
## Vzorový soubor

![PointAtChordLength](./Autodesk.DesignScript.Geometry.Curve.PointAtChordLength_img.jpg)

