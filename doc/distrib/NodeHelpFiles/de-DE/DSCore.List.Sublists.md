## Im Detail
`List.Sublists` gibt basierend auf dem eingegebenen Bereich und dem Versatz eine Reihe von Unterlisten aus einer angegebenen Liste zurück. Der Bereich bestimmt die Elemente der Eingabeliste, die in der ersten Unterliste platziert werden. Ein Versatz wird auf den Bereich angewendet, und der neue Bereich bestimmt die zweite Unterliste. Dieser Vorgang wird wiederholt, wobei der Startindex des Bereichs um den angegebenen Versatz erhöht wird, bis die resultierende Unterliste leer ist.

Im folgenden Beispiel beginnen wir mit einem Zahlenbereich von 0 bis 9. Der Bereich 0 bis 5 wird als Unterlistenbereich mit einem Versatz von 2 verwendet. In der Ausgabe verschachtelter Unterlisten enthält die erste Liste die Elemente mit Indizes im Bereich 0..5 und die zweite Liste die Elemente mit Indizes 2..7. Wenn dies wiederholt wird, werden die nachfolgenden Unterlisten kürzer, wenn das Ende des Bereichs die Länge der anfänglichen Liste überschreitet.
___
## Beispieldatei

![List.Sublists](./DSCore.List.Sublists_img.jpg)
