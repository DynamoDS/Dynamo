## Informacje szczegółowe
Węzeł Replace By Condition pobiera listę i sprawdza każdy element (item) pod kątem podanego warunku (condition). Jeśli wynik sprawdzania warunku to prawda (true), odpowiedni element zostaje zastąpiony na liście wyjściowej elementem określonym za pomocą pozycji wejściowej replaceWith. W poniższym przykładzie używamy węzła Formula i wprowadzamy formułę x%2==0, która wyszukuje resztę z dzielenia danego elementu przez 2, a następnie sprawdzamy, czy reszta jest równa zeru. Ta formuła zwraca wartość prawda (true) dla parzystych liczb całkowitych. Wartość wejściowa x pozostaje pusta. Użycie tej formuły jako warunku w węźle ReplaceByCondition powoduje utworzenie listy wyjściowej, na której każda liczba parzysta jest zastąpiona określonym elementem, w tym przypadku liczbą całkowitą 10.
___
## Plik przykładowy

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

