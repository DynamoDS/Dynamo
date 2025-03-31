## Im Detail
Der Block `Curve Mapper` nutzt mathematische Kurven, um Punkte innerhalb eines definierten Bereichs neu zu verteilen. Neuverteilung bedeutet in diesem Zusammenhang, dass X-Koordinaten auf Grundlage ihrer Y-Koordinaten neuen Positionen entlang einer bestimmten Kurve zugewiesen werden. Diese Technik ist besonders nützlich für Anwendungen wie Fassadengestaltung, parametrische Dachkonstruktionen und andere Entwurfsberechnungen, bei denen bestimmte Muster oder Verteilungen erforderlich sind.

Definieren Sie die Grenzwerte für die X- und Y-Koordinaten, indem Sie die minimalen und maximalen Werte festlegen. Diese Grenzwerte definieren die Grenzen, innerhalb derer die Punkte neu verteilt werden. Wählen Sie als Nächstes eine mathematische Kurve aus den bereitgestellten Optionen aus. Dazu zählen lineare, Sinus-, Kosinus-, Perlin-Noise-, Bezier-, Gaußsche, parabolische, Quadratwurzel- und Potenzkurven. Verwenden Sie die interaktiven Steuerpunkte, um die Form der ausgewählten Kurve entsprechend Ihren spezifischen Anforderungen anzupassen.

Sie können die Kurvenform mit der Schaltfläche Sperren sperren, wodurch weitere Änderungen der Kurve verhindert werden. Darüber hinaus können Sie die Form auf den Vorgabezustand zurücksetzen, indem Sie die Schaltfläche Zurücksetzen innerhalb des Blocks verwenden.

Geben Sie die Anzahl der neu zu verteilenden Punkte an, indem Sie die Eingabe Count festlegen. Der Block berechnet neue X-Koordinaten für die angegebene Anzahl an Punkten, basierend auf der ausgewählten Kurve und den definierten Grenzwerten. Die Punkte werden so neu verteilt, dass ihre X-Koordinaten der Form der Kurve entlang der Y-Achse folgen.

Um beispielsweise 80 Punkte entlang einer Sinuskurve neu zu verteilen, legen Sie Min. X. auf 0, Max. X auf 20, Min. Y auf 0 und Max. Y auf 10 fest. Nachdem Sie die Sinuskurve ausgewählt und ihre Form nach Bedarf angepasst haben, gibt der Block `Curve Mapper` 80 Punkte mit X-Koordinaten aus, die dem Sinuskurvenmuster entlang der Y-Achse von 0 bis 10 folgen.




___
## Beispieldatei

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
