## Informacje szczegółowe
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Zdefiniuj granice dla współrzędnych x i y, ustawiając wartości minimalne i maksymalne. Granice te wyznaczają obwiednie, w obrębie których nastąpi ponowne rozmieszczenie punktów. Następnie wybierz krzywą matematyczną z dostępnych opcji, takich jak krzywa liniowa, sinusoidalna, kosinusoidalna, szumu Perlina, Beziera, Gaussa, paraboliczna, pierwiastka kwadratowego i potęgowa. Użyj interaktywnych punktów sterujących, aby dopasować kształt wybranej krzywej, dostosowując ją do konkretnych potrzeb.

Kształt krzywej można zablokować za pomocą przycisku blokady, co zapobiega dalszym modyfikacjom krzywej. Ponadto można przywrócić stan domyślny kształtu za pomocą przycisku resetowania wewnątrz węzła.

Określ liczbę punktów do ponownego rozmieszczenia, ustawiając wartość wejściową Count. Węzeł oblicza nowe współrzędne x dla określonej liczby punktów w oparciu o wybraną krzywą i zdefiniowane granice. Punkty są rozmieszczane w taki sposób, aby ich współrzędne x podążały za kształtem krzywej wzdłuż osi y.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## Plik przykładowy


