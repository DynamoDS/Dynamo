## Im Detail
Extend With Arc fügt einen kreisförmigen Bogen am Start oder Ende einer eingegebenen PolyCurve hinzu und gibt eine einzelne kombinierte PolyCurve zurück. Die Radiuseingabe bestimmt den Radius des Kreises, während die Längeneingabe den Abstand entlang des Kreises für den Bogen bestimmt. Die Gesamtlänge muss kleiner oder gleich der Länge eines vollständigen Kreises mit dem angegebenen Radius sein. Der generierte Bogen ist tangential zum Ende der eingegebenen PolyCurve. Eine boolesche Eingabe für endOrStart steuert, an welchem Ende der PolyCurve der Bogen erstellt wird. Der Wert True ergibt den Bogen am Ende der PolyCurve, während False den Bogen am Start der PolyCurve erstellt. Im folgenden Beispiel werden zunächst ein Satz zufälliger Punkte und PolyCurve by Points zum Erstellen einer PolyCurve verwendet. Verwenden Sie dann zwei Zahlen-Schieberegler und einen booleschen Schalter, um die Parameter für ExtendWithArc festzulegen.
___
## Beispieldatei

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

