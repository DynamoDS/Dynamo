## Im Detail
`Mesh.Sphere` erstellt eine Netzkugel, zentriert am eingegebenen `origin`-Punkt, mit einem bestimmten `radius` und einer bestimmten Anzahl von `divisions`. Die boolesche Eingabe für `icosphere` wird verwendet, um zwischen den kugelförmigen Netztypen `icosphere` und `UV-Sphere` zu wechseln. Ein Ikosphärennetz bedeckt die Kugel mit regelmäßigeren Dreiecken als ein UV-Netz und führt in der Regel zu besseren Ergebnissen bei folgenden Modellierungsvorgängen. Bei einem UV-Netz sind die Pole an der Kugelachse ausgerichtet, und die Dreiecks-Layer werden in Längsrichtung um die Achse erzeugt.

Im Falle der Ikosphäre kann die Anzahl der Dreiecke um die Kugelachse so niedrig wie die angegebene Anzahl von Unterteilungen und höchstens doppelt so groß sein. Die Unterteilungen einer `UV-sphere` bestimmen die Anzahl der Dreiecks-Layer, die in Längsrichtung um die Kugel generiert werden. Wenn die Eingabe für `divisions` auf null gesetzt ist, gibt der Block eine UV-Kugel mit einer Vorgabeanzahl von 32 Unterteilungen für beide Netztypen zurück.

Im folgenden Beispiel wird der Block `Mesh.Sphere` verwendet, um zwei Kugeln mit identischem Radius und identischen Unterteilungen, jedoch mit unterschiedlichen Methoden zu erstellen. Wenn die Eingabe für `icosphere` auf `True` gesetzt ist, gibt `Mesh.Sphere` eine `icosphere` zurück. Wenn alternativ die Eingabe für `icosphere` auf `False` gesetzt ist, gibt der Block `Mesh.Sphere` eine `UV-sphere` zurück.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
