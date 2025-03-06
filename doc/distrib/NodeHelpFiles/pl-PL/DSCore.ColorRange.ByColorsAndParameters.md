## Informacje szczegółowe
Węzeł ByColorsAndParameters tworzy zakres kolorów 2D na podstawie listy kolorów wejściowych (colors) i odpowiadającej jej listy określonych parametrów UV z zakresu od 0 do 1 (parameters). W poniższym przykładzie za pomocą węzła Code Block tworzymy trzy różne kolory (w tym przypadku po prostu zielony, czerwony i niebieski) i łączymy je w listę. Za pomocą innego węzła Code Block tworzymy trzy parametry UV, po jednym dla każdego koloru. Te dwie listy są używane jako dane wejściowe węzła ByColorsAndParameters. Używamy kolejnego węzła GetColorAtParameter oraz węzła Display.ByGeometryColor, aby zwizualizować ten zakres kolorów 2D na zestawie sześcianów.
___
## Plik przykładowy

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

