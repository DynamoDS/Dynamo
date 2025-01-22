## Informacje szczegółowe
Węzeł `List.TakeEveryNthItem` tworzy nową listę zawierającą tylko elementy z listy wejściowej, które znajdują się w odstępach o wartości wejściowej n. Punkt początkowy odstępu można zmienić za pomocą pozycji danych wejściowych `offset`. Na przykład podanie n=3 i pozostawienie wartości `offset` jako domyślnej, czyli równej 0, spowoduje zachowanie elementów z indeksami 2, 5, 8 itp. Przy wartości `offset` równej 1 zachowane zostaną elementy z indeksami 0, 3, 6 itp. Uwaga: wartość `offset` działa z „zawijaniem” na całej liście. Aby usunąć wybrane elementy, zamiast je zachować, użyj węzła `List.DropEveryNthItem`.

W poniższym przykładzie najpierw generujemy listę liczb przy użyciu węzła `Range`, a następnie zachowujemy co drugą liczbę, używając jako parametru wejściowego n wartości 2.
___
## Plik przykładowy

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
