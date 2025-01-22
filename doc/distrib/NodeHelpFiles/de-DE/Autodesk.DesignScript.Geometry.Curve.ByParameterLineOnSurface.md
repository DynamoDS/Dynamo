## Im Detail
Curve by Parameter Line On Surface erstellt eine Linie entlang einer Oberfläche zwischen zwei eingegebenen UV-Koordinaten. Im folgenden Beispiel erstellen Sie zuerst ein Raster aus Punkten und verschieben diese um einen zufälligen Betrag in Z-Richtung. Diese Punkte werden zur Erstellung einer Oberfläche mithilfe eines NurbsSurface.ByPoints-Blocks verwendet. Diese Oberfläche wird als baseSurface eines ByParameterLineOnSurface-Blocks verwendet. Eine Reihe von Zahlen-Schiebereglern wird verwendet, um die U- und V-Eingaben von zwei UV.ByCoordinates-Blöcken anzupassen, die dann zur Bestimmung des Start- und Endpunkts der Linie auf der Oberfläche verwendet werden.
___
## Beispieldatei

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

