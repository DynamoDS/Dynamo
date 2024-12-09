## Informacje szczegółowe
Węzeł Color Range tworzy gradient między kolorami z zestawu wejściowego i umożliwia wybranie kolorów z tego gradientu za pomocą listy wartości wejściowych. Pierwsza pozycja wejściowa, colors, to lista kolorów do użycia w gradiencie. Druga pozycja wejściowa, indices, określa względne położenie kolorów wejściowych w gradiencie. Lista ta powinna być zgodna z listą kolorów, a każda wartość powinna być z zakresu od 0 do 1. Dokładna wartość nie jest istotna, ważne jest tylko względne położenie wartości. Kolor odpowiadający najniższej wartości znajdzie się po lewej stronie gradientu, a kolor odpowiadający najwyższej wartości znajdzie się po prawej stronie gradientu. Ostatnia pozycja wejściowa, value, umożliwia wybranie punktów wzdłuż gradientu z zakresu od 0 do 1 do wyprowadzenia na wyjściu. W poniższym przykładzie najpierw tworzymy dwa kolory: czerwony i niebieski. Kolejność tych kolorów w gradiencie jest określana przez listę utworzoną za pomocą węzła Code Block. Trzeci węzeł Code Block służy do utworzenia zakresu liczb między 0 a 1, które wyznaczą kolory wyjściowe z gradientu. Generowany jest zestaw sześcianów wzdłuż osi X — te sześciany są na końcu kolorowane zgodnie z gradientem za pomocą węzła Display.ByGeometryColor.
___
## Plik przykładowy

![Color Range](./CoreNodeModels.ColorRange_img.jpg)

