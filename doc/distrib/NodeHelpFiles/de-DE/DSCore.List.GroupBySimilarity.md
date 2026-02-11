## Im Detail
`List.GroupBySimilarity` gruppiert Listenelemente basierend auf der Nachbarschaft ihrer Indizes und der Ähnlichkeit ihrer Werte. Die Liste der zu gruppierenden Elemente kann entweder Zahlen (Ganzzahlen und Gleitkommazahlen) oder Zeichenfolgen enthalten, jedoch nicht beides.

Verwenden Sie die Eingabe `tolerance`, um die Ähnlichkeit von Elementen zu bestimmen. Bei Nummernlisten stellt der Wert 'tolerance' die maximal zulässige Differenz zwischen zwei Nummern dar, damit sie als ähnlich angesehen werden.

Bei Zeichenfolgenlisten stellt 'tolerance' die maximale Anzahl an Zeichen dar, die sich bei zwei Zeichenfolgen unterscheiden können, wobei die Levenshtein-Distanz zum Vergleich verwendet wird. Die maximale Toleranz für Zeichenfolgen beträgt 10.

Die boolesche Eingabe `considerAdjacency` gibt an, ob die Nachbarschaft beim Gruppieren der Elemente berücksichtigt werden soll. Wenn True angegeben ist, werden nur benachbarte Elemente, die sich ähneln, gruppiert. Wenn False angegeben ist, wird nur die Ähnlichkeit zur Bildung von Gruppen verwendet, unabhängig von der Nachbarschaft.

Der Block gibt eine Liste mit Listen gruppierter Werte basierend auf Nachbarschaft und Ähnlichkeit sowie eine Liste mit Listen der Indizes der gruppierten Elemente in der ursprünglichen Liste aus.

Im folgenden Beispiel wird `List.GroupBySimilarity` auf zwei Arten verwendet: zum Gruppieren einer Liste mit Zeichenfolgen nur nach Ähnlichkeit und zum Gruppieren einer Liste mit Nummern nach Nachbarschaft und Ähnlichkeit.
___
## Beispieldatei

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
