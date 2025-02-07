## Im Detail

Das Heat Series-Plot erstellt ein Diagramm, in dem Datenpunkte als Rechtecke in verschiedenen Farben entlang eines Farbbereichs dargestellt werden.

Weisen Sie jeder Spalte und Zeile Beschriftungen zu, indem Sie eine Liste mit Zeichenfolgenbeschriftungen in den X-Beschriftungs- und Y-Beschriftungseingaben eingeben. Die Anzahl der X-Beschriftungen und der Y-Beschriftungen muss nicht übereinstimmen.

Definieren Sie einen Wert für jedes Rechteck mit der Werteingabe. Die Anzahl der Unterlisten muss der Anzahl der Zeichenfolgenwerte in der X-Beschriftungseingabe entsprechen, da diese die Anzahl der Spalten darstellt. Die Werte in den einzelnen Unterlisten stellen die Anzahl der Rechtecke in jeder Spalte dar. Beispiel: 4 Unterlisten entsprechen 4 Spalten, und wenn jede Unterliste 5 Werte enthält, hat jede Spalte 5 Rechtecke.

Weiteres Beispiel: Um ein Raster mit 5 Zeilen und 5 Spalten zu erstellen, geben Sie 5 Zeichenfolgenwerte in der X-Beschriftungseingabe und der Y-Beschriftungseingabe an. Die X-Beschriftungswerte werden unterhalb des Diagramms entlang der X-Achse, die Y-Beschriftungswerte links neben dem Diagramm entlang der Y-Achse angezeigt.

Geben Sie in der Werteingabe eine Liste von Listen ein, wobei jede Unterliste 5 Werte enthält. Die Werte werden spaltenweise von links nach rechts und von unten nach oben geplottet, sodass der erste Wert in der ersten Unterliste das untere Rechteck in der linken Spalte, der zweite Wert das Rechteck darüber usw. ist. Jede Unterliste steht für eine Spalte im Plot.

Sie können einen Farbbereich zuweisen, um die Datenpunkte zu unterscheiden, indem Sie eine Liste mit Farbwerten in die Farbeingabe eingeben. Der niedrigste Wert im Diagramm entspricht der ersten Farbe, und der höchste Wert entspricht der letzten Farbe, die anderen Werte liegen dazwischen entlang des Verlaufs. Wenn kein Farbbereich zugewiesen ist, erhalten die Datenpunkte eine zufällige Farbe von der hellsten zur dunkelsten Schattierung.

Die besten Ergebnisse erzielen Sie, wenn Sie eine oder zwei Farben verwenden. Die Beispieldatei enthält ein klassisches Beispiel mit zwei Farben: Blau und Rot. Wenn sie als Farbeingaben verwendet werden, erstellt das Heat Series-Plot automatisch einen Verlauf zwischen diesen Farben, wobei niedrige Werte in Blautönen und hohe Werte in Rottönen dargestellt werden.

___
## Beispieldatei

![Heat Series Plot](./CoreNodeModelsWpf.Charts.HeatSeriesNodeModel_img.jpg)

