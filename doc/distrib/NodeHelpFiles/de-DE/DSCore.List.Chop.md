## Im Detail
`List.Chop` teilt eine bestimmte Liste basierend auf einer Liste von eingegebenen ganzzahligen Längen in kleinere Listen auf. Die erste verschachtelte Liste enthält die Anzahl der Elemente, die durch die erste Zahl in der Eingabe `lengths` angegeben wird. Die zweite verschachtelte Liste enthält die Anzahl der Elemente, die durch die zweite Zahl in der Eingabe lengths angegeben wird, usw. `List.Chop` wiederholt die letzte Zahl in der Eingabe `length`, bis alle Elemente aus der Eingabeliste geteilt wurden.

Im folgenden Beispiel wird ein Codeblock verwendet, um einen Zahlenbereich von 0 bis 5 in Schritten von 1 zu generieren. Diese Liste enthält 6 Elemente. Mithilfe eines zweiten Codeblocks wird eine Liste mit Längen erstellt, in die die erste Liste unterteilt wird. Die erste Zahl in dieser Liste lautet 1, mit der `List.Chop` eine verschachtelte Liste mit 1 Element erstellt. Die zweite Zahl lautet 3, wodurch eine verschachtelte Liste mit 3 Elementen erstellt wird. Da keine weiteren Längen angegeben sind, nimmt `List.Chop` alle verbleibenden Elemente in die dritte und letzte verschachtelte Liste auf.
___
## Beispieldatei

![List.Chop](./DSCore.List.Chop_img.jpg)
