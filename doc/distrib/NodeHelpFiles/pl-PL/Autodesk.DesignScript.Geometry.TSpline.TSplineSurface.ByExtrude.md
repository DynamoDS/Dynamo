## In-Depth
W poniższym przykładzie zostaje utworzona powierzchnia T-splajn jako wyciągnięcie proste danej krzywej `curve` profilu. Krzywa może być otwarta lub zamknięta. Wyciągnięcie jest wykonywane w podanym kierunku `direction` i może być realizowane w obu kierunkach — sterują tym pozycje danych wejściowych `frontDistance` i `backDistance`. Rozpiętości można ustawić osobno dla dwóch kierunków wyciągnięcia za pomocą pozycji danych wejściowych `frontSpans` i `backSpans`. Na potrzeby ustalenia definicji powierzchni wzdłuż krzywej pozycja danych wejściowych `profileSpans` steruje liczbą powierzchni, a pozycja `uniform` rozkłada je równomiernie lub uwzględnia krzywiznę. Wreszcie pozycja `inSmoothMode` steruje tym, czy powierzchnia jest wyświetlana w trybie gładkim, czy w trybie ramki.

## Plik przykładowy
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
