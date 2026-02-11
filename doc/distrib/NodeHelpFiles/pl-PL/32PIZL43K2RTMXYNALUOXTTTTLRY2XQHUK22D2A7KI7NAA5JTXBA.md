<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## Informacje szczegółowe
Węzeł `Curve.ExtrudeAsSolid (curve, direction)` wyciąga zamkniętą płaską krzywą wejściową (curve), określając kierunek wyciągnięcia za pomocą wektora wejściowego (direction). Długość wektora jest używana do określenia odległości wyciągnięcia. Ten węzeł zamyka końce wyciągnięcia w celu utworzenia bryły.

W poniższym przykładzie najpierw tworzymy krzywą NurbsCurve za pomocą węzła `NurbsCurve.ByPoints` z danymi wejściowymi w postaci zestawu losowo wygenerowanych punktów. Za pomocą węzła Code Block określamy składowe X, Y i Z węzła `Vector.ByCoordinates`. Tego wektora używamy następnie jako kierunku wejściowego węzła `Curve.ExtrudeAsSolid`.
___
## Plik przykładowy

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
