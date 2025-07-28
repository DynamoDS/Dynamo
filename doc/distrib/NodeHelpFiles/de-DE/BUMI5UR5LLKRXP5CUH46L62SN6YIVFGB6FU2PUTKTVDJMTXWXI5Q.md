## Im Detail

Im folgenden Beispiel wird eine T-Spline-Oberfläche unter Verwendung eines `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)`-Blocks an eine Kante einer BRep-Oberfläche angeglichen. Die für den Block mindestens erforderliche Eingabe ist die Basiseingabe `tSplineSurface`, ein Satz von Kanten der Oberfläche, die in der Eingabe `tsEdges` bereitgestellt werden, und eine Kante oder eine Liste von Kanten, die in der Eingabe `brepEdges` bereitgestellt werden. Die folgenden Eingaben steuern die Parameter der Angleichung:
- Mit `continuity` kann der Kontinuitätstyp für die Angleichung festgelegt werden. Bei der Eingabe werden Werte von 0, 1 oder 2 erwartet, die der G0-Positions-, G1-Tangenten- und G2-Krümmungsstetigkeit entsprechen.
- `useArcLength` steuert die Optionen für den Ausrichtungstyp. Wenn True festgelegt ist, wird der Ausrichtungstyp Bogenlänge verwendet. Diese Ausrichtung minimiert den physischen Abstand zwischen jedem Punkt der T-Spline-Oberfläche und dem entsprechenden Punkt auf der Kurve. Wenn False eingegeben wird, lautet der Ausrichtungstyp Parametrisch - jeder Punkt auf der T-Spline-Oberfläche wird an einen Punkt mit vergleichbarem parametrischen Abstand entlang der entsprechenden Zielkurve angeglichen.
- Wenn `useRefinement` auf True gesetzt ist, werden der Oberfläche Steuerpunkte hinzugefügt, um dem Ziel innerhalb eines bestimmten `refinementTolerance`-Werts zu entsprechen
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## Beispieldatei

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
