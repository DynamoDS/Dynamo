## Im Detail
`List.FilterByBoolMask` verwendet zwei Listen als Eingabe. Die erste Liste wird entsprechend einer zugehörigen Liste boolescher Werte (True oder False) in zwei separate Listen unterteilt. Elemente aus der Eingabe `list`, die in der Eingabe `mask` dem Wert True entsprechen, werden an die Ausgabe mit der Bezeichnung In weitergeleitet, während die Elemente, die einem False-Wert entsprechen, an die Ausgabe mit der Bezeichnung `out` weitergeleitet werden.

Im folgenden Beispiel wird `List.FilterByBoolMask` verwendet, um Holz und Laminat aus einer Liste mit Materialien auszuwählen. Zuerst werden zwei Listen verglichen, um passende Elemente zu finden, und dann wird ein `Or`-Operator verwendet, um nach Listenelementen mit dem Wert True zu suchen. Anschließend werden die Listenelemente gefiltert, je nachdem, ob es sich um Holz oder Laminat oder ein anderes Material handelt.
___
## Beispieldatei

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
