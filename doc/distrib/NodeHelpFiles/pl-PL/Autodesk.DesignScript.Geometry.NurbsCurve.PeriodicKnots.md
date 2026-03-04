## Informacje szczegółowe
Metody „NurbsCurve.PeriodicKnots” należy użyć, gdy trzeba wyeksportować zamkniętą krzywą NURBS do innego układu (na przykład Alias) lub system oczekuje krzywej w postaci okresowej. Wiele narzędzi CAD oczekuje tej postaci w celu zagwarantowania dokładności przenoszenia danych w obie strony.

Metoda „PeriodicKnots” zwraca wektor węzła w postaci *okresowej* (nieprzyciętej). Metoda „Knots” zwraca go w postaci *przyciętej*. Oba szyki mają tę samą długość. Są to dwa różne sposoby opisania tej samej krzywej. W formie przyciętej węzły powtarzają się na początku i końcu, aby krzywa była związana z zakresem parametrów. W postaci okresowej odstępy między węzłami powtarzają się na początku i końcu, co daje gładko zamkniętą pętlę.

W poniższym przykładzie jest tworzona okresowa krzywa NURBS za pomocą metody „NurbsCurve.ByControlPointsWeightsKnots”. Węzły obserwacyjne porównują parametry „Knots” i „PeriodicKnots”, co pozwala zilustrować tę samą długość różnymi wartościami. Parametr Knots to postać przycięta (powtarzające się węzły na końcach), a parametr PeriodicKnots to postać nieprzycięta z powtarzającym się wzorem różnicy, który definiuje okresowość krzywej.
___
## Plik przykładowy

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
