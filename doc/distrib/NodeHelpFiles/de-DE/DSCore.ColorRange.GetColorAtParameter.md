## Im Detail
GetColorAtParameter verwendet einen eingegebenen 2D-Farbbereich und gibt eine Liste mit Farben an angegebenen UV-Parametern im Bereich 0 bis 1 zurück. Im folgenden Beispiel erstellen Sie zunächst einen 2D-Farbbereich mithilfe eines ByColorsAndParameters-Blocks mit einer Liste von Farben und Parametern, um den Bereich festzulegen. Ein Codeblock wird verwendet, um einen Zahlenbereich zwischen 0 und 1 zu generieren, der als U- und V-Eingabe in einem UV.ByCoordinates-Block verwendet wird. Die Vergitterung dieses Blocks ist auf Kartesisches Produkt festgelegt. Eine Reihe von Würfeln wird auf ähnliche Weise erstellt, die ein Point.ByCoordinates-Block mit einer Vergitterung vom Typ Kartesisches Produkt zum Erstellen eines Würfel-Arrays verwendet hat. Anschließend wird ein Display.ByGeometryColor-Block mit dem Würfel-Array und der Liste der Farben verwendet, die aus dem GetColorAtParameter-Block abgerufen wurden.
___
## Beispieldatei

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

