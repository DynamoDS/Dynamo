## Im Detail

Im folgenden Beispiel wird eine T-Spline-Oberfläche unter Verwendung des
Blocks `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)` an eine NURBS-Kurve angeglichen. Die erforderliche Mindesteingabe für den
Block ist der Basiswert für `tSplineSurface`, ein Satz von Kanten der Oberfläche, bereitgestellt in der Eingabe `tsEdges`, und eine Kurve oder
Liste mit Kurven.
Die folgenden Eingaben steuern die Parameter der Angleichung:
- Mit `continuity` kann der Kontinuitätstyp für die Angleichung festgelegt werden. Für die Eingabe werden Werte von 0, 1 oder 2 erwartet, die der G0-Positions-, G1-Tangenten- und G2-Krümmungsstetigkeit entsprechen. Für die Angleichung einer Oberfläche an eine Kurve ist jedoch nur G0 (Eingabewert 0) verfügbar.
- `useArcLength` steuert die Optionen für den Ausrichtungstyp. Wenn True festgelegt ist, wird der Ausrichtungstyp Bogenlänge
verwendet. Diese Ausrichtung minimiert den physischen Abstand zwischen jedem Punkt der T-Spline-Oberfläche und
dem entsprechenden Punkt auf der Kurve. Wenn False eingegeben wird, lautet der Ausrichtungstyp Parametrisch -
jeder Punkt auf der T-Spline-Oberfläche wird an einen Punkt mit vergleichbarem parametrischen Abstand entlang der
entsprechenden Zielkurve angeglichen.
- Wenn `useRefinement` auf True gesetzt ist, werden der Oberfläche Steuerpunkte hinzugefügt, um das Ziel
innerhalb eines bestimmten `refinementTolerance`-Werts anzugleichen
- `numRefinementSteps`ist die maximale Anzahl von Unterteilungen der T-Spline-Basisfläche
beim Versuch, den Wert `refinementTolerance` zu erreichen. `numRefinementSteps` und `refinementTolerance` werden ignoriert, wenn `useRefinement` auf False gesetzt ist.
- `usePropagation` steuert, welcher Teil der Oberfläche von der Angleichung betroffen ist. Wenn False festgelegt ist, ist die Oberfläche nur minimal betroffen. Wenn True festgelegt ist, ist die Oberfläche innerhalb des angegebenen Abstands für `widthOfPropagation` betroffen.
- `scale` ist die Tangentenskalierung, die sich auf die Ergebnisse für die G1- und G2-Stetigkeit auswirkt.
- `flipSourceTargetAlignment` kehrt die Ausrichtungsrichtung um.


## Beispieldatei

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
