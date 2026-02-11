## Informacje szczegółowe
Węzeł From Pixels z pozycjami wejściowymi szerokości (width) i wysokości (height) tworzy na podstawie wejściowej płaskiej listy kolorów obraz, w którym każdy kolor jest jednym pikselem. Wynik przemnożenia szerokości przez wysokość powinien być równy całkowitej liczbie kolorów. W poniższym przykładzie najpierw tworzymy listę kolorów za pomocą węzła ByARGB. Węzeł Code Block tworzy zakres wartości od 0 do 255, który po połączeniu z pozycjami wejściowymi r i g tworzy szereg kolorów od czarnego do żółtego. Tworzony jest obraz o szerokości 8. Za pomocą węzłów Count i Division określamy wysokość obrazu. Za pomocą węzła Watch Image możemy wyświetlić podgląd utworzonego obrazu.
___
## Plik przykładowy

![FromPixels (colors, width, height)](./DSCore.IO.Image.FromPixels(colors,%20width,%20height)_img.jpg)

