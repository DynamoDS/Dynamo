## Informacje szczegółowe
Węzeł From Pixels tworzy obiekt obrazu (image) z wejściowej dwuwymiarowej tablicy kolorów. W poniższym przykładzie najpierw za pomocą węzła Code Block generujemy zakres liczb z przedziału od 0 do 255. Węzeł Color.ByARGB tworzy kolory z tego zakresu, przy czym skratowanie tego węzła jest ustawione na iloczyn wektorowy w celu utworzenia tablicy dwuwymiarowej. Następnie używamy węzła Image.FromPixels do utworzenia obrazu. Za pomocą węzła Watch Image możemy wyświetlić podgląd utworzonego obrazu.
___
## Plik przykładowy

![FromPixels (colors)](./DSCore.IO.Image.FromPixels(colors)_img.jpg)

