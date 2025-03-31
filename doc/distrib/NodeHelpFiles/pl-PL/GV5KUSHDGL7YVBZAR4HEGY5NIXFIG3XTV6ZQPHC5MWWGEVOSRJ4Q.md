## Informacje szczegółowe
Węzeł `Curve Mapper` wykorzystuje krzywe matematyczne do ponownego rozmieszczania punktów w zdefiniowanym zakresie. Ponowne rozmieszczanie w tym kontekście oznacza ponowne przypisanie współrzędnych x do nowych pozycji wzdłuż określonej krzywej na podstawie ich współrzędnych y. Technika ta jest szczególnie przydatna w takich zastosowaniach, jak projektowanie elewacji i parametrycznych konstrukcji dachu oraz inne obliczenia projektowe, w których wymagane są określone wzorce lub rozkłady.

Zdefiniuj granice dla współrzędnych x i y, ustawiając wartości minimalne i maksymalne. Granice te wyznaczają obwiednie, w obrębie których nastąpi ponowne rozmieszczenie punktów. Następnie wybierz krzywą matematyczną z dostępnych opcji, takich jak krzywa liniowa, sinusoidalna, kosinusoidalna, szumu Perlina, Beziera, Gaussa, paraboliczna, pierwiastka kwadratowego i potęgowa. Użyj interaktywnych punktów sterujących, aby dopasować kształt wybranej krzywej, dostosowując ją do konkretnych potrzeb.

Kształt krzywej można zablokować za pomocą przycisku blokady, co zapobiega dalszym modyfikacjom krzywej. Ponadto można przywrócić stan domyślny kształtu za pomocą przycisku resetowania wewnątrz węzła.

Określ liczbę punktów do ponownego rozmieszczenia, ustawiając wartość wejściową Count. Węzeł oblicza nowe współrzędne x dla określonej liczby punktów w oparciu o wybraną krzywą i zdefiniowane granice. Punkty są rozmieszczane w taki sposób, aby ich współrzędne x podążały za kształtem krzywej wzdłuż osi y.

Aby na przykład rozmieścić 80 punktów wzdłuż krzywej sinusoidalnej, ustaw wartość Min X równą 0, Max X równą 20, Min Y równą 0 oraz Max Y równą 10. Po wybraniu krzywej sinusoidalnej i dopasowaniu jej kształtu węzeł `Curve Mapper` zwraca 80 punktów ze współrzędnymi x, które podążają za wzorcem krzywej sinusoidalnej wzdłuż osi Y w zakresie od 0 do 10.




___
## Plik przykładowy

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
