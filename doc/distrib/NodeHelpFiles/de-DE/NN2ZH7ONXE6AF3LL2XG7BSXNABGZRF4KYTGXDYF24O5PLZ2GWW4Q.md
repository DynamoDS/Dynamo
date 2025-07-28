<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## Im Detail
Die Modi Quader und Glatt bieten zwei Möglichkeiten zur Anzeige einer T-Spline-Oberfläche. Der Modus Glatt stellt die tatsächliche Form einer T-Spline-Oberfläche dar und eignet sich zur Vorschau der Ästhetik und der Bemaßungen des Modells. Im Modus Quader hingegen können Sie Einblicke in die Oberflächenstruktur gewinnen und diese besser verstehen. Außerdem ist dies die schnellere Option, um eine Vorschau großer oder komplexer Geometrie anzuzeigen. Mit dem Block `TSplineSurface.EnableSmoothMode` können Sie während verschiedener Phasen der Geometrieentwicklung zwischen diesen beiden Vorschaustatus wechseln.

Im folgenden Beispiel wird der Abschrägungsvorgang für eine T-Spline-Quaderoberfläche durchgeführt. Das Ergebnis wird zunächst im Modus Quader visualisiert (Eingabe `inSmoothMode` der Quaderoberfläche auf False gesetzt), um die Struktur der Form besser zu verstehen. Anschließend wird der Modus Glatt über den Block `TSplineSurface.EnableSmoothMode` aktiviert, und das Ergebnis wird auf die rechte Seite verschoben, um gleichzeitig eine Vorschau beider Modi anzuzeigen.
___
## Beispieldatei

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
