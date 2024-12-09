## Informacje szczegółowe
Węzeł `Curve.SplitByParameter (curve, parameters)` pobiera jako dane wejściowe krzywą (curve) i listę parametrów (parameters). Dzieli krzywą w określonych wartościach parametrów i zwraca listę wynikowych krzywych.

W poniższym przykładzie najpierw tworzymy krzywą NurbsCurve za pomocą węzła `NurbsCurve.ByControlPoints` z danymi wejściowymi w postaci zestawu losowo wygenerowanych punktów. Za pomocą węzła Code Block tworzymy serię liczb z przedziału od 0 do 1 używanych jako lista parametrów, przy których krzywa jest dzielona.

___
## Plik przykładowy

![SplitByParameter](./Autodesk.DesignScript.Geometry.Curve.SplitByParameter_img.jpg)

