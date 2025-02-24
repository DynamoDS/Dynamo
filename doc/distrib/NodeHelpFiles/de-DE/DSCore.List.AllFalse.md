## Im Detail
`List.AllFalse` gibt False zurück, wenn ein Element in der angegebenen Liste True lautet oder kein boolescher Wert ist. `List.AllFalse` gibt nur True zurück, wenn jedes Element in der angegebenen Liste ein boolescher Wert ist und False lautet.

Im folgenden Beispiel wird `List.AllFalse` verwendet, um Listen mit booleschen Werten auszuwerten. Die erste Liste weist den Wert True auf, daher wird False zurückgegeben. Die zweite Liste weist nur False-Werte auf, daher wird True zurückgegeben. Die dritte Liste enthält eine Unterliste mit dem Wert True, sodass False zurückgegeben wird. Der letzte Block wertet die beiden Unterlisten aus und gibt False für die erste Unterliste zurück, da sie den Wert True enthält. Für die zweite Unterliste wird True zurückgegeben, da sie nur False-Werte enthält.
___
## Beispieldatei

![List.AllFalse](./DSCore.List.AllFalse_img.jpg)
