## Im Detail
`Solid.Repair` versucht, Volumenkörper mit ungültiger Geometrie zu reparieren und nach Möglichkeit Optimierungen vorzunehmen. `Solid.Repair` gibt ein neues Volumenkörperobjekt zurück.

Dieser Block ist hilfreich, wenn Sie Fehler beim Ausführen von Vorgängen mit importierter oder konvertierter Geometrie feststellen.

Im folgenden Beispiel wird `Solid.Repair` verwendet, um Geometrie aus einer **SAT-Datei** zu reparieren. Boolesche Operationen für die Geometrie in der Datei schlagen fehl, oder die Geometrie kann nicht gestutzt werden, und `Solid.Repair` bereinigt jegliche *ungültige Geometrie*, die den Fehler verursacht.

Im Allgemeinen sollten Sie diese Funktion nicht für Geometrie verwenden müssen, die Sie in Dynamo erstellen, sondern nur für Geometrie aus externen Quellen. Wenn Sie feststellen, dass dies nicht der Fall ist, melden Sie einen Fehler an den Dynamo-Team-GitHub
___
## Beispieldatei

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
