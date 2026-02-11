## Im Detail
`List.Transpose` vertauscht die Zeilen und Spalten in einer Liste mit Listen. Eine Liste mit 5 Unterlisten mit je 10 Elementen würde beispielsweise in 10 Listen mit je 5 Elementen umgewandelt. Nullwerte werden nach Bedarf eingefügt, um sicherzustellen, dass jede Unterliste dieselbe Anzahl von Elementen enthält.

In diesem Beispiel wird eine Liste mit Zahlen von 0 bis 5 und eine weitere Liste mit Buchstaben von A bis E generiert. Anschließend werden diese mit `List.Create` kombiniert. `List.Transpose` generiert 6 Listen mit je 2 Elementen, eine Zahl und einen Buchstaben pro Liste. Beachten Sie, dass `List.Transpose` für das nicht gepaarte Element einen Nullwert eingefügt hat, da eine der ursprünglichen Listen länger als die andere war.
___
## Beispieldatei

![List.Transpose](./DSCore.List.Transpose_img.jpg)
