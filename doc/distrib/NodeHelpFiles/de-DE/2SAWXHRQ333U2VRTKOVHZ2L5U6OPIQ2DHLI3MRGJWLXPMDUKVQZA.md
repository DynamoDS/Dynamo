<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## Im Detail
Erstellt eine NurbsSurface mit den angegebenen Kontrollscheitelpunkten, Knoten, Gewichtungen sowie U- und V-Gradwerten. Für die Daten gelten einige Einschränkungen; werden diese nicht eingehalten, schlägt die Funktion fehl, und ein Ausnahmefehler wird ausgegeben. Grad: Sowohl U als auch V müssen höheren Grades als 1 (stückweise linearer Spline) und niedrigeren Grades als 26 (maximal von ASM unterstützter Grad der B-Spline-Basis) sein. Gewichtungen: Alle Gewichtungswerte (falls angegeben) müssen streng positiv sein. Gewichtungen kleiner als 1e-11 werden nicht akzeptiert, und die Funktion schlägt fehl. Knoten: Beide Knotenvektoren müssen nicht-absteigende Folgen sein. Die Einflussverstärkung der inneren Knoten darf nicht größer sein als Grad plus 1 am Start- und Endknoten und Grad an den inneren Knoten (dies ermöglicht die Darstellung von Oberflächen mit G1-Diskontinuität). Beachten Sie, dass ungeklammerte Knotenvektoren unterstützt werden, sie werden jedoch in geklammerte konvertiert, und die entsprechenden Änderungen werden auf die Kontrollpunkt-/Gewichtungsdaten angewendet.
___
## Beispieldatei



