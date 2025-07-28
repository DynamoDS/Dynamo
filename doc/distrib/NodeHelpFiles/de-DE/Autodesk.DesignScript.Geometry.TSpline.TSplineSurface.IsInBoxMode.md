## Im Detail
Die Modi Quader und Glatt bieten zwei Möglichkeiten zur Anzeige einer T-Spline-Oberfläche. Der Modus Glatt stellt die tatsächliche Form einer T-Spline-Oberfläche dar und eignet sich zur Vorschau der Ästhetik und der Bemaßungen des Modells. Im Modus Quader hingegen können Sie Einblicke in die Oberflächenstruktur gewinnen und diese besser verstehen. Außerdem ist dies die schnellere Option, um eine Vorschau großer oder komplexer Geometrie anzuzeigen. Die Modi Quader und Glatt können Sie über Blöcke wie `TSplineSurface.EnableSmoothMode` zum Zeitpunkt der Erstellung der anfänglichen T-Spline-Oberfläche oder später steuern.

Wenn ein T-Spline ungültig wird, wechselt die Vorschau automatisch in den Modus Quader. Der Block `TSplineSurface.IsInBoxMode` ist eine weitere Möglichkeit, um zu ermitteln, ob die Oberfläche ungültig wird.

Im folgenden Beispiel wird eine T-Spline-Ebenenoberfläche erstellt, wobei die Eingabe `smoothMode` auf True gesetzt ist. Zwei der Flächen werden gelöscht, wodurch die Oberfläche ungültig wird. Die Oberflächenvorschau wechselt in den Modus Quader, obwohl dies allein anhand der Vorschau nicht zu erkennen ist. Der Block `TSplineSurface.IsInBoxMode` wird verwendet, um zu bestätigen, dass sich die Oberfläche im Modus Quader befindet.
___
## Beispieldatei

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
