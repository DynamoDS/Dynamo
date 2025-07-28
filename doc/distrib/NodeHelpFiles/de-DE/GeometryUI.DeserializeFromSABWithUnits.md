## Im Detail
`Geometry.DeserializeFromSABWithUnits` importiert Geometrie aus einem SAB-Byte-Array (Standard ACIS Binary) und aus dem `DynamoUnit.Unit`-Objekt, das aus Millimetern konvertiert werden kann, in Dynamo. Dieser Block benötigt als erste Eingabe byte[] und als zweite Eingabe `dynamoUnit`. Wenn die `dynamoUnit`-Eingabe null lautet, wird die SAB-Geometrie als einheitenlos importiert, wobei die geometrischen Daten im Array ohne Einheitenkonvertierung importiert werden. Wenn eine Einheit angegeben wird, werden die internen Einheiten des SAB-Arrays in die angegebenen Einheiten konvertiert.

Dynamo weist keine Einheiten auf, die numerischen Werte im Dynamo-Diagramm verfügen jedoch wahrscheinlich noch über eine implizite Einheit. Sie können die Eingabe `dynamoUnit` verwenden, um die interne Geometrie der SAB-Datei auf dieses Einheitensystem zu skalieren.

Im folgenden Beispiel wird aus SAB ein Quader mit 2 Maßeinheiten (einheitenlos) erstellt. Die `dynamoUnit`-Eingabe skaliert die ausgewählte Einheit für die Verwendung in anderer Software.

___
## Beispieldatei

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
