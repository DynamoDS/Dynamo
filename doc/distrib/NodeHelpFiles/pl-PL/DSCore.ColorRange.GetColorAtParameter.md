## Informacje szczegółowe
Węzeł GetColorAtParameter pobiera zakres kolorów 2D i zwraca listę kolorów dla określonych parametrów UV w zakresie od 0 do 1. W poniższym przykładzie najpierw tworzymy zakres kolorów 2D za pomocą węzła ByColorsAndParameters na podstawie listy kolorów i listy parametrów do ustawienia zakresu. Za pomocą węzła Code Block generujemy zakres liczb między 0 a 1, który służy jako dane wejściowe u i v dla węzła UV.ByCoordinates. Skratowanie tego węzła jest ustawione na iloczyn wektorowy. W podobny sposób tworzymy zestaw sześcianów: węzeł Point.ByCoordinates ze skratowaniem ustawionym na iloczyn wektorowy tworzy tablicę sześcianów. Następnie używamy węzła Display.ByGeometryColor z tablicą sześcianów i listą kolorów uzyskanych z węzła GetColorAtParameter.
___
## Plik przykładowy

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

