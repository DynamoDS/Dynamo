## Im Detail
Die Operation `Mesh.MakeHollow` kann verwendet werden, um ein Netzobjekt als Vorbereitung für den 3D-Druck auszuhöhlen. Das Aushöhlen eines Netzes kann die Menge des erforderlichen Druckmaterials, die Druckzeit und die Kosten erheblich reduzieren. Die Eingabe für `wallThickness` definiert die Wandstärke des Netzobjekts. Optional kann `Mesh.MakeHollow` Fluchtlöcher generieren, um überschüssiges Material während des Druckvorgangs zu entfernen. Die Größe und Anzahl der Löcher wird durch die Eingaben für `holeCount` und `holeRadius` gesteuert. Schließlich wirken sich die Eingaben für `meshResolution` und `solidResolution` auf die Auflösung des Netzergebnisses aus. Ein höherer Wert für `meshResolution` verbessert die Genauigkeit, mit der der innere Teil des Netzes das ursprüngliche Netz versetzt, führt jedoch zu mehr Dreiecken. Ein höherer Wert für `solidResolution` verbessert das Ausmaß, in dem feinere Details des ursprünglichen Netzes auf dem inneren Teil des ausgehöhlten Netzes erhalten bleiben.
Im folgenden Beispiel wird `Mesh.MakeHollow` für ein Netz in Form eines Kegels verwendet. An seiner Basis werden fünf Fluchtlöcher hinzugefügt.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
