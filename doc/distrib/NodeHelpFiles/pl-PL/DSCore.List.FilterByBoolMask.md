## Informacje szczegółowe
Węzeł `List.FilterByBoolMask` pobiera dwie listy jako dane wejściowe. Pierwsza lista (list) jest podzielona na dwie oddzielne listy zgodnie z odpowiednią listą wartości logicznych (True lub False). Elementy z listy wejściowej `list` odpowiadające wartościom True (prawda) na liście wejściowej `mask` są kierowane do danych wyjściowych oznaczonych jako `in`, natomiast elementy odpowiadające wartościom False (fałsz) są kierowane do danych wyjściowych oznaczonych jako `out`.

W poniższym przykładzie węzeł `List.FilterByBoolMask` służy do wybierania drewna i laminatu z listy materiałów. Najpierw porównujemy dwie listy, aby znaleźć zgodne elementy, a następnie używamy operatora `Or`, aby sprawdzić pod kątem elementów listy o wartości True (prawda). Następnie elementy listy są filtrowane w zależności od tego, czy dotyczą drewna, laminatu, czy czegoś innego.
___
## Plik przykładowy

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
