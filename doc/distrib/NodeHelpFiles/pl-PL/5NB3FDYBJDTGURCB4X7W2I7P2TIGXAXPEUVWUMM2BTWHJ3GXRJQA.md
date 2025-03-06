<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## Informacje szczegółowe
Węzeł `Curve.Extrude (curve, direction, distance)` wyciąga krzywą wejściową (curve), określając kierunek wyciągnięcia za pomocą wektora wejściowego (direction). Do określenia odległości wyciągnięcia jest używana osobna pozycja wejściowa `distance`.

W poniższym przykładzie najpierw tworzymy krzywą NurbsCurve za pomocą węzła `NurbsCurve.ByControlPoints` z danymi wejściowymi w postaci zestawu losowo wygenerowanych punktów. Za pomocą węzła Code Block określamy składowe X, Y i Z węzła `Vector.ByCoordinates`. Tego wektora używamy następnie jako pozycji danych wejściowych `direction` węzła `Curve.Extrude`, a do sterowania pozycją wejściową `distance` używamy węzła `number slider`.
___
## Plik przykładowy

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
