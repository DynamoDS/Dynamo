## Informacje szczegółowe
Węzeł `List.DropEveryNthItem` usuwa elementy z listy wejściowej (list) w odstępach wartości wejściowej „n”. Punkt początkowy odstępu można zmienić za pomocą danych wejściowych `offset`. Na przykład podanie n=3 i pozostawienie wartości offset jako domyślnej, czyli równej 0, spowoduje usunięcie elementów z indeksami 2, 5, 8 itp. Przy wartości offset równej 1 usunięte zostaną elementy z indeksami 0, 3, 6 itp. Uwaga: wartość offset działa z „zawijaniem” na całej liście. Aby zachować wybrane elementy, zamiast je usuwać, użyj węzła `List.TakeEveryNthItem`.

W poniższym przykładzie najpierw generujemy listę liczb przy użyciu węzła `Range`, a następnie usuwamy co drugą liczbę, używając jako parametru wejściowego `n` wartości 2.
___
## Plik przykładowy

![List.DropEveryNthItem](./DSCore.List.DropEveryNthItem_img.jpg)
