## Informacje szczegółowe
Węzeł IsNull zwraca wartość logiczną na podstawie tego, czy obiekt ma wartość null. W poniższym przykładzie rysowana jest siatka okręgów z różnymi promieniami na podstawie poziomu koloru czerwonego w bitmapie. Jeśli nie ma wartości czerwonej, nie jest rysowany żaden okrąg i zwracana jest wartość null na liście okręgów. Przekazanie tej listy do węzła IsNull powoduje zwrócenie listy wartości logicznych, na której wartość prawda (true) reprezentuje każde położenie wartości null. Tej listy wartości logicznych można użyć z węzłem List.FilterByBoolMask, aby zwrócić listę bez wartości null.
___
## Plik przykładowy

![IsNull](./DSCore.Object.IsNull_img.jpg)

