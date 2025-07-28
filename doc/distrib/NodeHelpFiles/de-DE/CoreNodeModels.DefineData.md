## Im Detail

Der DefineData-Block validiert den Datentyp eingehender Daten. Er kann verwendet werden, um sicherzustellen, dass lokale Daten den gewünschten Typ haben, und um als Eingabe- oder Ausgabeblock verwendet zu werden, der den Datentyp deklariert, den ein Diagramm erwartet oder bereitstellt. Der Block unterstützt eine Reihe gängiger Dynamo-Datentypen, zum Beispiel Zeichenfolge, Punkt oder Boolesch. Die vollständige Liste der unterstützten Datentypen finden Sie im Dropdown-Menü des Blocks. Der Block unterstützt Daten in Form eines einzelnen Werts oder als flache Liste. Verschachtelte Listen, Wörterbücher und Replikationen werden nicht unterstützt.

### Verhalten

Der Block validiert die Daten, die vom Eingabeanschluss kommen, basierend auf der Einstellung des Dropdown-Menüs Typ und der Umschaltoption für Liste (weitere Informationen finden Sie unten). Wenn die Validierung erfolgreich ist, ist die Ausgabe des Blocks dieselbe wie die Eingabe. Wenn die Validierung nicht erfolgreich ist, geht der Block in einen Warnzustand mit einer Null-Ausgabe über.
Der Block verfügt über eine Eingabe:

-   Eingabe "**>**": Stellen Sie eine Verbindung zu einem vorgelagerten Block her, um den Datentyp zu validieren.
    Außerdem bietet der Block drei Benutzersteuerelemente:
-   Die Umschaltoption **Typ automatisch erkennen**: Wenn die Option aktiviert ist, analysiert der Block die eingehenden Daten, und wenn der Typ dieser Daten unterstützt wird, legt der Block die Steuerelementwerte für Typ und Liste basierend auf den eingehenden Daten fest. Das Dropdown-Menü Typ und die Umschaltoption Liste sind deaktiviert und werden basierend auf dem Eingabeblock automatisch aktualisiert.

    Wenn die Option Typ automatisch erkennen deaktiviert ist, können Sie einen Datentyp über das Menü Typ und die Umschaltoption Liste festlegen. Entsprechen die eingehenden Daten nicht Ihren Angaben, geht der Block in einen Warnstatus mit Null-Ausgabe über.
-   Dropdown-Menü **Typ***: Legt den erwarteten Datentyp fest. Wenn dieses Steuerelement aktiviert ist (Umschaltoption **Typ automatisch erkennen** ist deaktiviert), legen Sie einen Datentyp für die Validierung fest. Ist das Steuerelement deaktiviert (Umschaltoption **Typ automatisch erkennen** ist aktiviert), wird der Datentyp basierend auf eingehenden Daten automatisch festgelegt. Daten sind gültig, wenn der Typ genau mit dem angezeigten Typ übereinstimmt oder wenn der Typ ein untergeordnetes Element des angezeigten Typs ist. (Wenn z. B. das Dropdown-Menü Typ auf Kurve festgelegt ist, sind Objekte des Typs Rechteck, Linie usw. gültig.)
-   Umschaltoption **Liste**: Wenn diese Option aktiviert ist, erwartet der Block eingehende Daten als eine flache Liste mit Elementen eines gültigen Datentyps (siehe oben). Wenn die Option deaktiviert ist, erwartet der Block ein einzelnes Element eines gültigen Datentyps.

### Als Eingabeblock verwenden

Wenn die Option als Eingabe festgelegt wurde (Ist Eingabe im Kontextmenü des Blocks), kann der Block optional vorgelagerte Blöcke verwenden, um den Vorgabewert für die Eingabe festzulegen. Eine Ausführung des Diagramms speichert den Wert des DefineData-Blocks im Cache, damit er verwendet werden kann, wenn das Diagramm extern ausgeführt wird, z. B. mit dem Engine-Block.

## Beispieldatei

Im folgenden Beispiel ist die Option **Typ automatisch erkennen** der ersten Gruppe von "DefineData"-Blöcken deaktiviert. Der Block validiert die bereitgestellte Zahleneingabe ordnungsgemäß, während die Zeichenfolgeneingabe zurückgewiesen wird. Die zweite Gruppe enthält einen Block mit aktivierter Option **Typ automatisch erkennen**. Der Block passt die Dropdown-Liste Typ und die Umschaltoption Liste automatisch an, um der Eingabe, in diesem Fall einer Liste von Ganzzahlen, zu entsprechen.

![Define_Data](./CoreNodeModels.DefineData_img.png)
