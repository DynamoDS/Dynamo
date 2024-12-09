## Informacje szczegółowe
Węzeł `List.Chop` dzieli daną listę (list) na mniejsze listy na podstawie listy wejściowych długości całkowitych (lengths). Pierwsza lista zagnieżdżona zawiera liczbę elementów określoną przez pierwszą liczbę w danych wejściowych `lengths`. Druga lista zagnieżdżona zawiera liczbę elementów określoną przez drugą liczbę w danych wejściowych `lengths` itd. Węzeł `List.Chop` powtarza ostatnią liczbę w danych wejściowych `lengths`, dopóki nie zostaną pocięte wszystkie elementy z listy wejściowej.

W poniższym przykładzie za pomocą węzła Code Block generujemy zakres liczb z przedziału od 0 do 5 z przyrostem 1. Lista zawiera 6 elementów. Za pomocą drugiego węzła Code Block tworzymy listę długości, zgodnie z którą podzielimy pierwszą listę. Pierwsza liczba na tej liście to 1 — węzeł `List.Chop` utworzy listę zagnieżdżoną z 1 elementem. Druga liczba to 3 — zostanie utworzona lista zagnieżdżona z 3 elementami. Nie określono więcej długości, więc węzeł `List.Chop` umieści wszystkie pozostałe elementy na trzeciej, ostatniej liście zagnieżdżonej.
___
## Plik przykładowy

![List.Chop](./DSCore.List.Chop_img.jpg)
