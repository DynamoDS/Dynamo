<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## Podrobnosti
`Curve.TrimSegmentyByParameter (parameters, discardEvenSegment)` nejprve rozdělí křivku v bodech určených vstupním seznamem parametrů. Poté vrátí buď liché očíslované segmenty, nebo sudé očíslované segmenty, jak je určeno booleovskou hodnotou vstupu `discardEvenSegments`.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByControlPoints`, přičemž jako vstup použijeme sadu náhodně vygenerovaných bodů. Pomocí bloku kódu vytvoříme rozsah čísel mezi 0 a 1 s krokem 0.1. Jakmile tento rozsah použijeme jako vstupní parametry pro uzel `Curve.TrimSegmentsByParameter`, získáme seznam křivek, které jsou v podstatě přerušované verze původních křivek.
___
## Vzorový soubor

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
