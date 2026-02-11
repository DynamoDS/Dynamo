## Informacje szczegółowe
Węzeł `List.AllFalse` zwraca wartość False (fałsz), jeśli którykolwiek z elementów na danej liście (list) ma wartość True (prawda) lub nie jest wartością logiczną (Boolean). Węzeł `List.AllFalse` zwraca wartość True tylko w przypadku, gdy wszystkie elementy na liście są typu Boolean i mają wartość False.

W poniższym przykładzie użyjemy węzła `List.AllFalse` do oceny list wartości logicznych (Boolean). Pierwsza lista ma wartość True (prawda), więc zwracana jest wartość False (fałsz). Druga lista ma tylko wartości False, więc zwracana jest wartość True. Trzecia lista zawiera listę podrzędną z wartością True, więc zwracana jest wartość False. Końcowy węzeł ocenia dwie listy podrzędne i zwraca wartość False dla pierwszej z nich, ponieważ zawiera ona wartość True, oraz wartość True dla drugiej, ponieważ zawiera ona tylko wartości False.
___
## Plik przykładowy

![List.AllFalse](./DSCore.List.AllFalse_img.jpg)
