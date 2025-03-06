## Informacje szczegółowe

W poniższym przykładzie powierzchnia T-splajn jest dopasowywana do krawędzi powierzchni reprezentacji obwiedni za pomocą węzła `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)`. Minimalne dane wejściowe wymagane w przypadku tego węzła to podstawa `tSplineSurface`, zestaw krawędzi powierzchni podanych za pomocą pozycji danych wejściowych `tsEdges` oraz krawędź lub lista krawędzi podana za pomocą pozycji danych wejściowych `brepEdges`. Następujące dane wejściowe sterują parametrami dopasowania:
— `continuity`: umożliwia ustawienie typu ciągłości dla dopasowania. Ta pozycja danych wejściowych powinna mieć wartość 0, 1 lub 2, co odpowiada ciągłości pozycyjnej G0, stycznej G1 i krzywizny G2.
— `useArcLength`: steruje opcjami typu dopasowania. W przypadku ustawienia wartości True (Prawda) używany jest typ dopasowania Arc Length (Długość łuku). To dopasowanie minimalizuje odległość fizyczną między każdym punktem powierzchni T-splajn a odpowiednim punktem na krzywej. W przypadku ustawienia wartości False (Fałsz) typem dopasowania jest Parametric (Parametryczne) — każdy punkt na powierzchni T-splajn jest dopasowywany do punktu o porównywalnej odległości parametrycznej wzdłuż dopasowanej krzywej docelowej.
— `useRefinement`: w przypadku ustawienia wartości True (Prawda) dodaje do powierzchni punkty sterujące, aby spróbować dopasować cel w granicach danej tolerancji `refinementTolerance`
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## Plik przykładowy

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
