## Im Detail
`List.RemoveIfNot` gibt eine Liste zurück, in der Elemente, die dem angegebenen Elementtyp entsprechen, beibehalten und alle anderen Elemente aus der ursprünglichen Liste entfernt werden.

Möglicherweise müssen Sie in der Eingabe `type` den vollständigen Blockpfad verwenden, z. B. `Autodesk.DesignScript.Geometry.Surface`, um Elemente zu entfernen. Um die Pfade für Listenelemente abzurufen, können Sie die Liste in einen `Object.Type`-Block eingeben.

Im folgenden Beispiel gibt `List.RemoveIfNot` eine Liste mit einer Zeile zurück, wobei die Punktelemente aus der ursprünglichen Liste entfernt werden, da sie nicht dem angegebenen Typ entsprechen.
___
## Beispieldatei

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
