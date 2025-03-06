<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## Informacje szczegółowe
Węzeł Curve.ExtrudeAsSolid (direction, distance) wyciąga zamkniętą płaską krzywą wejściową (curve), określając kierunek wyciągnięcia za pomocą wektora wejściowego (direction). Do określenia odległości wyciągnięcia jest używana osobna pozycja wejściowa `distance`. Ten węzeł zamyka końce wyciągnięcia w celu utworzenia bryły.

W poniższym przykładzie najpierw tworzymy krzywą NurbsCurve za pomocą węzła `NurbsCurve.ByPoints` z danymi wejściowymi w postaci zestawu losowo wygenerowanych punktów. Za pomocą węzła `code block` określamy składowe X, Y i Z węzła `Vector.ByCoordinates`. Tego wektora używamy następnie jako kierunku wejściowego węzła `Curve.ExtrudeAsSolid`, a do sterowania pozycją wejściową `distance` używamy suwaka Number Slider.
___
## Plik przykładowy

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
