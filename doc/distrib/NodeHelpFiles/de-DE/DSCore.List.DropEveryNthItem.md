## Im Detail
`List.DropEveryNthItem` entfernt Elemente in Intervallen des eingegebenen n-Werts aus der Eingabeliste. Der Startpunkt des Intervalls kann mit der Eingabe `offset` geändert werden. Wenn Sie beispielsweise 3 in n eingeben und den Versatz als Vorgabe 0 beibehalten, werden Elemente mit den Indizes 2, 5, 8 usw. entfernt. Bei einem Versatz von 1 werden Elemente mit den Indizes 0, 3, 6 usw. entfernt. Beachten Sie, dass der Versatz die gesamte Liste umfasst. Informationen zum Beibehalten ausgewählter Elemente, anstatt sie zu entfernen, finden Sie unter `List.TakeEveryNthItem`.

Im folgenden Beispiel wird zunächst unter Verwendung von `Range` eine Liste mit Zahlen generiert. Anschließend wird jede zweite Zahl entfernt, indem 2 als Eingabe für `n` verwendet wird.
___
## Beispieldatei

![List.DropEveryNthItem](./DSCore.List.DropEveryNthItem_img.jpg)
