## Im Detail
`List.GroupByFunction` gibt eine neue, nach einer Funktion gruppierte Liste zurück.

Die `groupFunction`-Eingabe erfordert einen Block in einem Funktionszustand (d. h., er gibt eine Funktion zurück). Dies bedeutet, dass mindestens eine der Blockeingaben nicht verbunden ist. Dynamo führt dann die Blockfunktion für jedes Element in der Eingabeliste von `List.GroupByFunction` aus, um die Ausgabe als Gruppierungsmechanismus zu verwenden.

Im folgenden Beispiel werden zwei verschiedene Listen gruppiert, wobei `List.GetItemAtIndex` als Funktion verwendet wird. Diese Funktion erstellt Gruppen (eine neue Liste) aus jedem Index der obersten Ebene.
___
## Beispieldatei

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
