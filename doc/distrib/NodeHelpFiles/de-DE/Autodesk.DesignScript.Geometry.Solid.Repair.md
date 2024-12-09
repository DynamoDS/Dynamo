## Im Detail
Repair versucht, Volumenkörper mit ungültiger Geometrie zu reparieren und möglicherweise Optimierungen vorzunehmen. Der Repair-Block gibt ein neues Volumenkörperobjekt zurück.
Dieser Block ist hilfreich, wenn Sie Fehler beim Ausführen von Vorgängen mit importierter oder konvertierter Geometrie feststellen.

Wenn Sie beispielsweise Daten aus einem Basisbauteil-Kontext wie **Revit** oder aus einer **SAT**-Datei importieren und feststellen, dass boolesche Operationen oder das Stutzen unerwartet fehlschlagen, kann es vorkommen, dass durch einen Reparaturvorgang die *ungültige Geometrie* bereinigt werden kann, die den Fehler verursacht.

Im Allgemeinen sollten Sie diese Funktion nicht für Geometrie verwenden müssen, die Sie in Dynamo erstellen, sondern nur für Geometrie aus externen Quellen. Wenn Sie feststellen, dass dies nicht der Fall ist, melden Sie dem Dynamo-GitHub-Team einen Fehler!
___


