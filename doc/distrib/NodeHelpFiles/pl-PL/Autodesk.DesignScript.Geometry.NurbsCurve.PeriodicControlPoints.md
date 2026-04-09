## Informacje szczegółowe
Metody „NurbsCurve.PeriodicControlPoints” należy użyć, gdy trzeba wyeksportować zamkniętą krzywą NURBS do innego układu (na przykład Alias) lub system oczekuje krzywej w postaci okresowej. Wiele narzędzi CAD oczekuje tej postaci w celu zagwarantowania dokładności przenoszenia danych w obie strony.

Metoda „PeriodicControlPoints” zwraca punkty kontrolne w postaci *okresowej*. Metoda „ControlPoints” zwraca je w postaci *przyciętej*. Oba szyki mają tę samą liczbę punktów. Są to dwa różne sposoby opisania tej samej krzywej. W postaci okresowej kilka ostatnich punktów sterujących odpowiada kilku pierwszym (tyle, ile wynosi stopień krzywej), więc krzywa zamyka się gładko. W postaci przyciętej jest używany inny układ, więc położenia punktów w dwóch szykach są różne.

W poniższym przykładzie jest tworzona okresowa krzywa NURBS za pomocą metody „NurbsCurve.ByControlPointsWeightsKnot”. Węzły obserwacji porównują parametry „ControlPoints” i „PeriodicControlPoints”, aby zilustrować tę samą długość przy różnych położeniach punktów. Punkty sterujące są wyświetlane w kolorze czerwonym, dzięki czemu łatwo jest je odróżnić od okresowych punktów sterujących, które są wyświetlane na czarno, w podglądzie tła.
___
## Plik przykładowy

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
