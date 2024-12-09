## Podrobnosti
Uzel Split By Points rozdělí vstupní křivku v určených bodech a vrátí seznam výsledných segmentů. Pokud určené body nejsou na křivce, tento uzel najde body podél křivky, které jsou nejblíže vstupním bodům, a rozdělí křivku v těchto výsledných bodech. V níže uvedeném příkladu nejprve vytvoříme křivku Nurbs pomocí uzlu ByPoints, přičemž jako vstup se použije sada náhodně generovaných bodů. Stejná sada bodů se použije jako seznam bodů v uzlu SplitByPoints. Výsledkem je seznam segmentů křivky mezi vygenerovanými body.
___
## Vzorový soubor

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

