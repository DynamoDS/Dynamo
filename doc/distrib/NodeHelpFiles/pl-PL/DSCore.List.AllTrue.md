## Informacje szczegółowe
Węzeł `List.AllTrue` zwraca wartość False (fałsz), jeśli którykolwiek z elementów na danej liście (list) ma wartość False lub nie jest wartością logiczną (Boolean). Węzeł `List.AllTrue` zwraca wartość True (prawda) tylko w przypadku, gdy wszystkie elementy na liście są typu Boolean i mają wartość True.

W poniższym przykładzie użyjemy węzła `List.AllTrue` do oceny list wartości logicznych (Boolean). Pierwsza lista ma wartość False (fałsz), więc zwracana jest wartość False. Druga lista ma tylko wartości True (prawda), więc zwracana jest wartość True. Trzecia lista zawiera listę podrzędną z wartością False, więc zwracana jest wartość False. Końcowy węzeł ocenia dwie listy podrzędne i zwraca wartość False dla pierwszej z nich, ponieważ zawiera ona wartość False, oraz wartość True dla drugiej, ponieważ zawiera ona tylko wartości True.
___
## Plik przykładowy

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
