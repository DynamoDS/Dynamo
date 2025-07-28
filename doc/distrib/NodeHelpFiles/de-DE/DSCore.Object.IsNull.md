## Im Detail
IsNull gibt einen booleschen Wert zurück, abhängig davon, ob ein Objekt null lautet. Im folgenden Beispiel wird ein Raster mit Kreisen und unterschiedlichen Radien gezeichnet, die auf dem Rot-Wert in einer Bitmap-Datei basieren. Wenn kein Rot-Wert vorhanden ist, wird kein Kreis gezeichnet, und es wird ein Nullwert in der Kreisliste zurückgegeben. Wenn diese Liste durch IsNull weitergegeben wird, wird eine Liste boolescher Werte zurückgegeben. True steht dabei für jede Position eines Nullwerts. Diese Liste boolescher Werte kann mit List.FilterByBoolMask verwendet werden, um eine Liste ohne Nullen zurückzugeben.
___
## Beispieldatei

![IsNull](./DSCore.Object.IsNull_img.jpg)

