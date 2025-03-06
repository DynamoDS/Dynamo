## Im Detail
`Geometry.ImportFromSATWithUnits` importiert Geometrie aus einer SAT-Datei und aus dem `DynamoUnit.Unit`-Objekt, das aus Millimetern konvertiert werden kann, in Dynamo. Dieser Block benötigt als erste Eingabe ein Dateiobjekt oder einen Dateipfad und als zweite Eingabe `dynamoUnit`. Wenn die `dynamoUnit`-Eingabe null lautet, wird die SAT-Geometrie als einheitenlos importiert, wobei die geometrischen Daten in der Datei einfach ohne Einheitenkonvertierung importiert werden. Wenn eine Einheit übergeben wird, werden die internen Einheiten der SAT-Datei in die angegebenen Einheiten konvertiert.

Dynamo weist keine Einheiten auf, die numerischen Werte im Dynamo-Diagramm verfügen jedoch wahrscheinlich noch über eine implizite Einheit. Sie können die Eingabe `dynamoUnit` verwenden, um die interne Geometrie der SAT-Datei auf dieses Einheitensystem zu skalieren.

Im folgenden Beispiel wird Geometrie aus einer SAT-Datei importiert, wobei die Einheit Fuß lautet. Damit diese Beispieldatei auf Ihrem Computer funktioniert, laden Sie diese SAT-Beispieldatei herunter und verweisen den Block `File Path` auf die ungültige SAT-Datei.

___
## Beispieldatei

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
