## Informacje szczegółowe
Węzeł `Curve.Extrude (curve, direction)` wyciąga krzywą wejściową (curve), określając kierunek wyciągnięcia za pomocą wektora wejściowego (direction). Długość wektora jest używana do określenia odległości wyciągnięcia.

W poniższym przykładzie najpierw tworzymy krzywą NurbsCurve za pomocą węzła `NurbsCurve.ByControlPoints` z danymi wejściowymi w postaci zestawu losowo wygenerowanych punktów. Za pomocą węzła Code Block określamy składowe X, Y i Z węzła `Vector.ByCoordinates`. Tego wektora używamy następnie jako pozycji danych wejściowych `direction` węzła `Curve.Extrude`.
___
## Plik przykładowy

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
