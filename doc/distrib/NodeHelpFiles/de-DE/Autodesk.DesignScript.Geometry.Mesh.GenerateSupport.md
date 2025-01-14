## Im Detail
Der Block `Mesh.GenerateSupport` wird verwendet, um Stützen zur Eingabe-Netzgeometrie hinzuzufügen, damit sie für den 3D-Druck vorbereitet sind. Stützen sind erforderlich, um Geometrie mit Überständen erfolgreich zu drucken, damit eine korrekte Haftfähigkeit von Schichten sichergestellt und ein Durchhängen des Materials während des Druckvorgangs verhindert werden kann. `Mesh.GenerateSupport` erkennt Überstände und generiert automatisch baumartige Stützen, die weniger Material verbrauchen und leichter entfernt werden können, da sie weniger Kontakt mit der gedruckten Oberfläche haben. In Fällen, in denen keine Überhänge erkannt werden, ist das Ergebnis des Blocks `Mesh.GenerateSupport` dasselbe Netz, gedreht und in eine optimale Ausrichtung für den Druck gebracht und in die XY-Ebene verschoben. Die Konfiguration der Stützen wird durch die Eingaben gesteuert:
- baseHeight - definiert die Dicke des untersten Stützenteil - die entsprechende Basis
- baseDiameter steuert die Größe der Stützenbasis
- postDiameter-Eingabe steuert die Größe der einzelnen Stützen in der Mitte
- tipHeight und tipDiameter steuern die Größe der Stützen an ihrer Spitze, die mit der gedruckten Oberfläche in Kontakt sind
Im folgenden Beispiel wird der Block `Mesh.GenerateSupport` verwendet, um Stützen zu einem Netz in der Form des Buchstabens ‘T’ hinzuzufügen.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)
