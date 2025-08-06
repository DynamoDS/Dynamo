## Informacje szczegółowe
Węzeł `Curve Mapper` ponownie rozmieszcza szereg wartości wejściowych w zdefiniowanym zakresie i wykorzystuje krzywe matematyczne do zamapowania ich wzdłuż określonej krzywej. W tym kontekście mapowanie oznacza ponowne rozmieszczenie wartości w taki sposób, że ich współrzędne x odpowiadają kształtowi krzywej wzdłuż osi y. Technika ta jest szczególnie przydatna w takich zastosowaniach, jak projektowanie elewacji i parametrycznych konstrukcji dachu oraz inne obliczenia projektowe, w których wymagane są określone wzorce lub rozkłady.

Zdefiniuj granice dla współrzędnych x i y, ustawiając wartości minimalne i maksymalne. Granice te wyznaczają obwiednie, w obrębie których nastąpi ponowne rozmieszczenie punktów. Możesz podać pojedynczą liczbę, aby wygenerować szereg równomiernie rozmieszczonych wartości, lub istniejący szereg wartości, który zostanie rozmieszczony wzdłuż kierunku x w określonym zakresie, a następnie zamapowany na krzywą.

Wybierz krzywą matematyczną z dostępnych opcji: krzywą liniową, krzywą sinusoidalną, krzywą kosinusoidalną, krzywą szumu Perlina, krzywą Beziera, krzywą Gaussa, krzywą paraboliczną, krzywą pierwiastkową lub krzywą potęgową. Użyj interaktywnych punktów sterowania, aby dostosować kształt wybranej krzywej, dopasowując ją do konkretnych potrzeb.

Możesz zablokować kształt krzywej za pomocą przycisku blokady, zapobiegając dalszym modyfikacjom krzywej. Możesz też zresetować kształt do stanu domyślnego za pomocą przycisku resetowania wewnątrz węzła. W przypadku otrzymania wyników NaN lub Null więcej informacji o tym, skąd się one biorą, znajdziesz [tutaj](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues).

Aby na przykład ponownie rozmieścić 80 punktów wzdłuż krzywej sinusoidalnej w zakresie od 0 do 20, ustaw wartość Min równą 0, Max równą 20 i Values równą 80. Po wybraniu krzywej sinusoidalnej i dopasowaniu jej kształtu węzeł `Curve Mapper` zwraca 80 punktów ze współrzędnymi X, które podążają za wzorcem krzywej sinusoidalnej wzdłuż osi Y.

Aby zamapować nierównomiernie rozmieszczone wartości wzdłuż krzywej Gaussa, ustaw minimalny i maksymalny zakres oraz podaj szereg wartości. Po wybraniu krzywej Gaussa i dopasowaniu jej kształtu węzeł `Curve Mapper` ponownie rozmieszcza szereg wartości wzdłuż współrzędnych X przy użyciu określonego zakresu i mapuje wartości wzdłuż wzoru krzywej. Aby uzyskać szczegółową dokumentację na temat działania tego węzła i ustawiania danych wejściowych, zapoznaj się z [tym wpisem w blogu](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) dotyczącym węzła Curve Mapper.




___
## Plik przykładowy

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
