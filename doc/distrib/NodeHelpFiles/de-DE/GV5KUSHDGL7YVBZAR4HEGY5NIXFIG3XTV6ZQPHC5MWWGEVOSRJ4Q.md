## Im Detail
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Definieren Sie die Grenzwerte für die X- und Y-Koordinaten, indem Sie die minimalen und maximalen Werte festlegen. Diese Grenzwerte definieren die Grenzen, innerhalb derer die Punkte neu verteilt werden. Wählen Sie als Nächstes eine mathematische Kurve aus den bereitgestellten Optionen aus. Dazu zählen lineare, Sinus-, Kosinus-, Perlin-Noise-, Bezier-, Gaußsche, parabolische, Quadratwurzel- und Potenzkurven. Verwenden Sie die interaktiven Steuerpunkte, um die Form der ausgewählten Kurve entsprechend Ihren spezifischen Anforderungen anzupassen.

Sie können die Kurvenform mit der Schaltfläche Sperren sperren, wodurch weitere Änderungen der Kurve verhindert werden. Darüber hinaus können Sie die Form auf den Vorgabezustand zurücksetzen, indem Sie die Schaltfläche Zurücksetzen innerhalb des Blocks verwenden.

Geben Sie die Anzahl der neu zu verteilenden Punkte an, indem Sie die Eingabe Count festlegen. Der Block berechnet neue X-Koordinaten für die angegebene Anzahl an Punkten, basierend auf der ausgewählten Kurve und den definierten Grenzwerten. Die Punkte werden so neu verteilt, dass ihre X-Koordinaten der Form der Kurve entlang der Y-Achse folgen.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## Beispieldatei


