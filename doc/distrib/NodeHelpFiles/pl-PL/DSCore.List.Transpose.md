## Informacje szczegółowe
Węzeł `List.Transpose` zamienia wiersze i kolumny na liście list. Na przykład lista zawierająca 5 list podrzędnych po 10 elementów każda zostanie zmieniona w 10 list po 5 elementów każda. W razie potrzeby wstawiane są wartości null, aby zapewnić, że każda lista podrzędna będzie miała taką samą liczbę elementów.

W przykładzie generujemy listę liczb z przedziału od 0 do 5 i inną listę liter od A do E. Następnie za pomocą węzła `List.Create` łączymy je. Węzeł `List.Transpose` generuje 6 list po 2 elementy każda, po jednej liczbie i jednej literze na listę. Ponieważ jedna z list pierwotnych była dłuższa niż druga, węzeł `List.Transpose` wstawia wartość null dla elementu bez pary.
___
## Plik przykładowy

![List.Transpose](./DSCore.List.Transpose_img.jpg)
