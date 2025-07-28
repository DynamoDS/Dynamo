## Im Detail

Das Balkendiagramm erstellt ein Diagramm mit vertikal ausgerichteten Balken. Balken können unter mehreren Gruppen organisiert sein und mit einer Farbkodierung beschriftet werden. Sie haben die Möglichkeit, eine einzelne Gruppe zu erstellen, indem Sie einen einzelnen double-Wert eingeben, oder mehrere Gruppen zu erstellen, indem Sie mehrere double-Werte pro Unterliste in der Werteingabe eingeben. Um Kategorien zu definieren, geben Sie eine Liste mit Zeichenfolgenwerten in der Beschriftungseingabe ein. Jeder Wert erstellt eine neue farbkodierte Kategorie.

Um jedem Balken einen Wert (Höhe) zuzuweisen, geben Sie eine Liste von Listen mit double-Werten in die Werteingabe ein. Jede Unterliste bestimmt die Anzahl der Balken und die Kategorie, zu der sie gehören, in der gleichen Reihenfolge wie die Beschriftungseingabe. Wenn Sie eine einzelne Liste mit double-Werten haben, wird nur eine Kategorie erstellt. Die Anzahl der Zeichenfolgenwerte in der Beschriftungseingabe muss der Anzahl der Unterlisten in der Werteingabe entsprechen.

Um jeder Kategorie eine Farbe zuzuweisen, fügen Sie eine Liste mit Farben in der Farbeingabe ein. Wenn Sie benutzerdefinierte Farben zuweisen, muss die Anzahl der Farben mit der Anzahl der Zeichenfolgenwerte in der Beschriftungseingabe übereinstimmen. Wenn keine Farben zugewiesen werden, werden zufällige Farben verwendet.

## Beispiel: Einzelne Gruppe

Angenommen, Sie möchten durchschnittliche Benutzerbewertungen für ein Element in den ersten drei Monaten des Jahres darstellen. Um dies zu veranschaulichen, benötigen Sie eine Liste mit drei Zeichenfolgenwerten, die mit den Monatsangaben für Januar, Februar und März beschriftet werden.
Für die Beschriftungseingabe stellen wir die folgende Liste in einem Codeblock bereit:

[“January”, “February”, “March”];

Sie können auch Zeichenfolgenblöcke verwenden, die mit dem Listenerstellungsblock verbunden sind, um die Liste zu erstellen.

Als Nächstes geben wir in die Werteingabe die durchschnittliche Benutzerbewertung für jeden der drei Monate als Liste von Listen ein:

[[3.5], [5], [4]];

Beachten Sie, dass drei Unterlisten erforderlich sind, da drei Beschriftungen existieren.

Wenn das Diagramm jetzt ausgeführt wird, wird das Balkendiagramm erstellt, wobei jeder farbige Balken die durchschnittliche Kundenbewertung für den Monat darstellt. Sie können die Vorgabefarben weiterhin verwenden oder eine Liste mit benutzerdefinierten Farben in der Farbeingabe eingeben.

## Beispiel: Mehrere Gruppen

Sie können die Gruppierungsfunktion des Balkendiagrammblocks nutzen, indem Sie in jeder Unterliste weitere Werte in die Werteingabe eingeben. In diesem Beispiel erstellen wir ein Diagramm, das die Anzahl der Türen in drei Varianten von drei Modellen (Modell A, Modell B und Modell C) darstellt.

Dazu geben wir zunächst die Beschriftungen an:

[“Model A”, “Model B”, “Model C”];

Als Nächstes geben wir Werte ein und stellen erneut sicher, dass die Anzahl der Unterlisten mit der Anzahl der Beschriftungen übereinstimmt:

[[17, 9, 13],[12,11,15],[15,8,17]];

Wenn Sie jetzt auf Ausführen klicken, erstellt der Balkendiagrammblock ein Diagramm mit drei Gruppen von Balken, die mit Index 0, 1 und 2 gekennzeichnet sind. In diesem Beispiel wird jeder Index (d. h. Gruppe) als Konstruktionsvariation betrachtet. Die Werte in der ersten Gruppe (Index 0) werden vom ersten Element in jeder Liste in der Werteingabe abgerufen, sodass die erste Gruppe 17 für Modell A, 12 für Modell B und 15 für Modell C enthält . Die zweite Gruppe (Index 1) verwendet den zweiten Wert in jeder Gruppe usw.

___
## Beispieldatei

![Bar Chart](./CoreNodeModelsWpf.Charts.BarChartNodeModel_img.jpg)

