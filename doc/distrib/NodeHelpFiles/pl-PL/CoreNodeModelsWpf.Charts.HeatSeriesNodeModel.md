## Informacje szczegółowe

Węzeł Heat Series Plot tworzy wykres termiczny, w którym punkty danych są reprezentowane jako prostokąty w różnych kolorach z zakresu kolorów.

Etykiety dla każdej kolumny i każdego wiersza przypisuje się, wprowadzając odpowiednio listę etykiet w postaci ciągów do wejść x-labels i y-labels. Liczba wartości x-labels i y-labels nie musi być zgodna.

Określ wartość dla każdego prostokąta za pomocą danych wejściowych wartości (values). Liczba list podrzędnych musi być zgodna z liczbą wartości w postaci ciągów w danych wejściowych x-labels, ponieważ reprezentuje liczbę kolumn. Wartości na każdej liście podrzędnej reprezentują liczbę prostokątów w danej kolumnie. Na przykład 4 listy podrzędne odpowiadają 4 kolumnom, a jeśli każda lista podrzędna ma 5 wartości, każda kolumna zawiera 5 prostokątów.

Inny przykład: aby utworzyć siatkę z 5 wierszami i 5 kolumnami, podaj 5 wartości w postaci ciągów w wejściu x-labels oraz w wejściu y-labels. Wartości x-labels pojawią się poniżej wykresu, wzdłuż osi x, a wartości y-labels pojawią się po lewej stronie wykresu, wzdłuż osi y.

W danych wejściowych wartości (values) wprowadź listę list, z których każda zawiera 5 wartości. Wartości są kreślone kolumnami od lewej do prawej i od dołu do góry, więc pierwsza wartość z pierwszej listy podrzędnej jest dolnym prostokątem w lewej kolumnie, a druga — prostokątem powyżej niego itd. Każda lista podrzędna reprezentuje kolumnę na wykresie.

Można przypisać zakres kolorów, aby ułatwić rozróżnienie punktów danych, wprowadzając listę wartości kolorów w danych wejściowych kolorów (colors). Najniższa wartość na wykresie będzie równa pierwszemu kolorowi, najwyższa wartość — ostatniemu, a wartości pośrednie wypadną na skali gradientu. Jeśli nie zostanie przypisany żaden zakres kolorów, punktom danych zostaną nadane kolory losowe od najjaśniejszego do najciemniejszego.

Aby uzyskać najlepsze wyniki, należy użyć jednego lub dwóch kolorów. Plik przykładowy zawiera klasyczny przykład zastosowania dwóch kolorów: niebieskiego i czerwonego. Gdy są one używane jako dane wejściowe koloru, węzeł Heat Series Plot automatycznie tworzy gradient między tymi kolorami, przy czym niskie wartości reprezentowane są za pomocą odcieni niebieskiego, a wysokie — odcieni czerwonego.

___
## Plik przykładowy

![Heat Series Plot](./CoreNodeModelsWpf.Charts.HeatSeriesNodeModel_img.jpg)

