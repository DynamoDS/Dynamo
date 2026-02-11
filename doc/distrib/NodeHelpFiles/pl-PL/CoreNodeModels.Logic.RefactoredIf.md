## Informacje szczegółowe
Węzeł If pełni rolę węzła kontroli warunkowej. Pozycja wejściowa „test” pobiera wartość logiczną, natomiast pozycje wejściowe „true” i „false” przyjmują dowolny typ danych. Jeśli pozycja „test” ma wartość prawda (true), węzeł zwraca element z pozycji wejściowej „true”, a jeśli pozycja „test” ma wartość fałsz (false), węzeł zwraca element z pozycji wejściowej „false”. W poniższym przykładzie najpierw generujemy listę liczb losowych z przedziału od zera do 99. Liczbą elementów na liście sterujemy za pomocą suwaka Integer Slider. Za pomocą węzła Code Block ze wzorem „x%a==0” sprawdzamy podzielność przez drugą liczbę, określoną przez drugi suwak Integer Slider. Generuje to listę wartości logicznych odpowiadających temu, czy elementy na liście losowej są podzielne przez liczbę określoną przez drugi suwak Integer Slider. Ta lista wartości logicznych jest używana jako pozycja wejściowa „test” dla węzła If. Jako pozycji wejściowej „true” używamy sfery domyślnej, a jako pozycji wejściowej „false” używamy prostopadłościanu domyślnego. Wynikiem węzła If jest lista sfer lub prostopadłościanów. Na koniec używamy węzła Translate, aby rozdzielić listę tych geometrii.

Węzeł IF replikuje we wszystkich węzłach JAK USTAWIONY NA NAJKRÓTSZE. Przyczyna tego jest widoczna w dołączonych przykładach, zwłaszcza gdy obserwuje się wyniki uzyskiwane, kiedy do węzła formuły zastosowano ustawienie NAJDŁUŻSZE i następuje przejście „krótkiej” gałęzi warunku. Zmiany te zostały również wprowadzone, aby umożliwić przewidywalne zachowanie w przypadku używania pojedynczych wartości logicznych lub listy wartości logicznych.
___
## Plik przykładowy

![If](./CoreNodeModels.Logic.RefactoredIf_img.jpg)

