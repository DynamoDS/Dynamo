<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## Podrobnosti
Uzel `Curve.NormalAtParameter (curve, param)` vrátí vektor zarovnaný se směrem normály v zadaném parametru křivky. Parametrizace křivky je měřena v rozsahu od 0 do 1, přičemž 0 představuje počátek křivky a 1 představuje konec křivky.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByControlPoints`, přičemž jako vstup bude použita sada náhodně generovaných bodů. K řízení vstupu `parameter` uzlu `Curve.NormalAtParameter` se použije posuvník nastavený na rozsah 0 až 1.
___
## Vzorový soubor

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
