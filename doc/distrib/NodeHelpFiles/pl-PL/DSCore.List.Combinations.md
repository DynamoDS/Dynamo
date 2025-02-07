## Informacje szczegółowe
Węzeł `List.Combinations` zwraca listę zagnieżdżoną ze wszystkimi możliwymi kombinacjami elementów na liście wejściowej (list) o danej długości (length). W przypadku kombinacji kolejność elementów nie ma znaczenia, dlatego lista wyjściowa (0,1) jest równoważna kombinacji (1,0). Jeśli jako dane wejściowe `replace` ustawiono wartość True (prawda), elementy zostaną zastąpione na liście pierwotnej, co pozwala na ich wielokrotne użycie w kombinacji.

W poniższym przykładzie za pomocą węzła Code Block generujemy zakres liczb z przedziału od 0 do 5 z przyrostem 1. Za pomocą węzła `List.Combinations` z wartością wejściową `length` równą 3 generujemy wszystkie sposoby łączenia 3 liczb z tego przedziału. Jako wartość logiczną `replace` ustawiono True (prawda), więc liczby będą używane wielokrotnie.
___
## Plik przykładowy

![List.Combinations](./DSCore.List.Combinations_img.jpg)
