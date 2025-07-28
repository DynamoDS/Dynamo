## Informacje szczegółowe

W poniższym przykładzie powierzchnia T-splajn jest dopasowywana do krzywej NURBS za pomocą
węzła `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)`. Minimalne dane wejściowe wymagane dla
węzła to podstawa `tSplineSurface`, zestaw krawędzi powierzchni podany za pomocą pozycji danych wejściowych `tsEdges` oraz krzywa lub
lista krzywych.
Następujące dane wejściowe sterują parametrami dopasowania:
— `continuity`: umożliwia ustawienie typu ciągłości dla dopasowania. Ta pozycja danych wejściowych powinna mieć wartość 0, 1 lub 2, co odpowiada ciągłości pozycyjnej G0, stycznej G1 i krzywizny G2. Jednak w przypadku dopasowywania powierzchni do krzywej dostępna jest tylko wartość G0 (wartość wejściowa 0).
— `useArcLength`: steruje opcjami typu dopasowania. W przypadku ustawienia wartości True (Prawda) używany jest typ dopasowania Arc
Length (Długość łuku). To dopasowanie minimalizuje odległość fizyczną między każdym punktem powierzchni T-splajn a
odpowiednim punktem na krzywej. W przypadku ustawienia wartości False (Fałsz) typem dopasowania jest Parametric (Parametryczne) —
każdy punkt na powierzchni T-splajn jest dopasowywany do punktu o porównywalnej odległości parametrycznej wzdłuż
dopasowanej krzywej docelowej.
— `useRefinement`: w przypadku ustawienia wartości True (Prawda) dodaje do powierzchni punkty sterujące, aby spróbować dopasować cel
w granicach danej tolerancji `refinementTolerance`
— `numRefinementSteps`: jest to maksymalna liczba utworzeń podpodziałów bazowej powierzchni T-splajn
podczas próby osiągnięcia wartości tolerancji `refinementTolerance`. Jeśli dla pozycji `useRefinement` ustawiono wartość False (Fałsz), pozycje `numRefinementSteps` i `refinementTolerance` są ignorowane.
— `usePropagation`: określa, na ile dopasowanie wpływa na powierzchnię. W przypadku ustawienia wartości False (Fałsz) wpływ na powierzchnię jest minimalny. W przypadku ustawienia wartości True (Prawda) wpływ na powierzchnię jest w granicach podanej odległości `widthOfPropagation`.
— `scale`: to skala styczności wpływająca na wyniki dla ciągłości G1 i G2.
— `flipSourceTargetAlignment` odwraca kierunek dopasowania.


## Plik przykładowy

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
