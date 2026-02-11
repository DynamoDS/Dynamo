## Informacje szczegółowe

Węzeł XY Line Plot (wykres liniowy XY) tworzy wykres z jedną lub większą liczbą linii kreślonych według wartości x i y. Oznacz linie lub zmień liczbę linii, wprowadzając listę etykiet w postaci ciągów w danych wejściowych etykiet (labels). Każda etykieta tworzy nową linię oznaczoną kolorem. W razie wprowadzenia tylko jednej wartości ciągu zostanie utworzona tylko jedna linia.

Aby określić położenia poszczególnych punktów na poszczególnych liniach, należy użyć listy list zawierającej wartości typu double dla wejść wartości x i y. Liczba wartości wejściowych x i y musi być równa. Ponadto liczba list podrzędnych musi zgadzać się z liczbą wartości w postaci ciągów w danych wejściowych etykiet (labels).
Aby na przykład utworzyć 3 linie, każdą z 5 punktami, przekaż listę z 3 wartościami w postaci ciągów w danych wejściowych etykiet (labels), aby nadać nazwy poszczególnym liniom, i przekaż 3 listy podrzędne z 5 wartościami typu double każda jako wartości x i y.

Linie można oznaczyć kolorami, wprowadzając listę kolorów w danych wejściowych kolorów (colors). W przypadku przypisywania kolorów niestandardowych liczba kolorów musi być zgodna z liczbą wartości ciągów w danych wejściowych etykiet (labels). W przypadku nieprzypisania kolorów zostaną użyte kolory losowe.

___
## Plik przykładowy

![XY Line Plot](./CoreNodeModelsWpf.Charts.XYLineChartNodeModel_img.jpg)

