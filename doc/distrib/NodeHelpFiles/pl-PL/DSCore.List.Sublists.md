## Informacje szczegółowe
Węzeł `List.Sublists` zwraca serię list podrzędnych z danej listy na podstawie zakresu danych wejściowych i odsunięcia. Zakres określa elementy listy wejściowej, które zostaną umieszczone na pierwszej liście podrzędnej. Do zakresu stosowane jest odsunięcie i nowy zakres określa drugą listę podrzędną. Proces ten jest powtarzany przez zwiększanie indeksu początkowego zakresu o dane odsunięcie, aż wynikowa lista podrzędna będzie pusta.

W poniższym przykładzie zaczynamy od zakresu liczb od 0 do 9. Jako zakresu listy podrzędnej używamy zakresu liczb od 0 do 5, a odsunięcie ma wartość 2. W danych wyjściowych zagnieżdżonych list podrzędnych pierwsza lista zawiera elementy z indeksami z zakresu od 0 do 5, a druga — z indeksami od 2 do 7. W miarę tego, jak jest to powtarzane, kolejne listy podrzędne stają się coraz krótsze, ponieważ koniec zakresu przekracza długość listy początkowej.
___
## Plik przykładowy

![List.Sublists](./DSCore.List.Sublists_img.jpg)
