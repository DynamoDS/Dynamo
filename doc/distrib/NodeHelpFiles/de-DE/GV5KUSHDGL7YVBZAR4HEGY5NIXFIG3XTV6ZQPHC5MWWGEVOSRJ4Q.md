## Im Detail
Der Block `Curve Mapper` verteilt eine Reihe von Eingabewerten innerhalb eines definierten Bereichs neu und nutzt mathematische Kurven, um sie entlang einer angegebenen Kurve zuzuordnen. Zuordnung bedeutet in diesem Zusammenhang, dass die Werte neu verteilt werden, sodass ihre X-Koordinaten der Form der Kurve auf der Y-Achse folgen. Diese Technik ist besonders nützlich für Anwendungen wie Fassadengestaltung, parametrische Dachkonstruktionen und andere Entwurfsberechnungen, bei denen bestimmte Muster oder Verteilungen erforderlich sind.

Definieren Sie die Grenzwerte für die X-Koordinaten, indem Sie die minimalen und maximalen Werte festlegen. Diese Grenzwerte definieren die Grenzen, innerhalb derer die Punkte neu verteilt werden. Sie können entweder eine einzelne Anzahl angeben, um eine Reihe von gleichmäßig verteilten Werten zu generieren, oder eine vorhandene Reihe von Werten, die entlang der X-Richtung innerhalb des angegebenen Bereichs verteilt und dann der Kurve zugeordnet werden.

Wählen Sie eine mathematische Kurve aus den bereitgestellten Optionen aus. Dazu zählen lineare, Sinus-, Kosinus-, Perlin-Noise-, Bezier-, Gaußsche, parabolische, Quadratwurzel- und Potenzkurven. Verwenden Sie die interaktiven Steuerpunkte, um die Form der ausgewählten Kurve entsprechend Ihren spezifischen Anforderungen anzupassen.

Sie können die Kurvenform mit der Schaltfläche Sperren sperren, wodurch weitere Änderungen der Kurve verhindert werden. Darüber hinaus können Sie die Form auf den Vorgabezustand zurücksetzen, indem Sie die Schaltfläche Zurücksetzen innerhalb des Blocks verwenden. Wenn Sie als Ergebnis NaN oder Null erhalten, finden Sie [hier](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues) weitere Informationen über die Gründe dafür.

Um beispielsweise 80 Punkte entlang einer Sinuskurve innerhalb eines Bereichs von 0 bis 20 neu zu verteilen, legen Sie Min. auf 0, Max. auf 20 und Werte auf 80 fest. Nachdem Sie die Sinuskurve ausgewählt und ihre Form nach Bedarf angepasst haben, gibt der Block `Curve Mapper` 80 Punkte mit X-Koordinaten aus, die dem Sinuskurvenmuster entlang der Y-Achse folgen.

Um ungleichmäßig verteilte Werte entlang einer Gaußkurve zuzuordnen, legen Sie den minimalen und maximalen Bereich fest und geben die Wertereihe an. Nachdem Sie die Gaußkurve ausgewählt und ihre Form nach Bedarf angepasst haben, verteilt der Block `Curve Mapper` die Wertereihe entlang der X-Koordinaten unter Verwendung des angegebenen Bereichs neu und ordnet die Werte entlang des Kurvenmusters zu. Eine ausführliche Dokumentation zur Funktionsweise des Blocks und zum Festlegen von Eingaben finden Sie in [diesem Blog-Beitrag](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) zum Curve Mapper.




___
## Beispieldatei

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
