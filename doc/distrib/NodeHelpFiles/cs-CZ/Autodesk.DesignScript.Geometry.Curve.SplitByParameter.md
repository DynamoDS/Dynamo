## Podrobnosti
Položka `Curve.SplitByParameter (curve, parameters)` přijímá jako vstupy křivku a seznam parametrů. Rozděluje křivku podle zadaných parametrů a vrací seznam výsledných křivek.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByControlPoints`, přičemž jako vstup bude použita sada náhodně generovaných bodů. Blok kódu slouží k vytvoření řady čísel v rozmezí od 0 do 1, která budou použita jako seznam parametrů, podle kterých je křivka rozdělena.

___
## Vzorový soubor

![SplitByParameter](./Autodesk.DesignScript.Geometry.Curve.SplitByParameter_img.jpg)

