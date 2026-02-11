## Informacje szczegółowe
Węzeł Prune Duplicates pobiera jako dane wejściowe listę punktów (points) i zwraca listę po usunięciu powielonych punktów. Wartość wejściowa tolerancji (tolerance) służy do określania, jak zbliżone muszą być dwa punkty, aby były traktowane jako powielenie. Jeśli dwa punkty są sobie bliższe, niż wynosi wartość tolerancji, pierwszy punkt z listy zostanie zachowany, a drugi punkt zostanie usunięty. W przykładzie generujemy zestaw punktów losowych. Następnie używamy węzła Prune Duplicates z tolerancją równą jednemu punktowi, aby usunąć wszystkie punkty zbliżone do innych punktów o mniej niż jeden punkt.
___
## Plik przykładowy

![PruneDuplicates](./Autodesk.DesignScript.Geometry.Point.PruneDuplicates_img.jpg)

