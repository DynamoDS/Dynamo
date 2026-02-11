## Informacje szczegółowe
Węzeł `Curve.Extrude (curve, distance)` wyciąga krzywą wejściową (curve), określając odległość wyciągnięcia za pomocą liczby wejściowej (distance). Kierunek wyciągnięcia jest określany za pomocą kierunku wektora normalnego wzdłuż krzywej.

W poniższym przykładzie najpierw tworzymy krzywą NurbsCurve za pomocą węzła `NurbsCurve.ByControlPoints` z danymi wejściowymi w postaci zestawu losowo wygenerowanych punktów. Następnie za pomocą węzła `Curve.Extrude` wyciągamy krzywą. Wartość wejściową `distance` węzła `Curve.Extrude` określamy za pomocą suwaka Number Slider.
___
## Plik przykładowy

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
