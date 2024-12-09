<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## Im Detail
`Surface.Thicken (surface, thickness, both_sides)` erstellt einen Volumenkörper durch Versetzen einer Oberfläche entsprechend der `thickness`-Eingabe und durch Verschließen der Enden, um den Volumenkörper zu schließen. Dieser Block verfügt über eine zusätzliche Eingabe, mit der angegeben wird, ob auf beiden Seiten verdickt werden soll. Die `both_sides`-Eingabe erfordert einen booleschen Wert: True zum Verdicken auf beiden Seiten und False zum Verdicken auf einer Seite. Beachten Sie, dass der Parameter `thickness` die Gesamtdicke des endgültigen Volumenkörpers bestimmt. Wenn daher `both_sides` auf True festgelegt ist, wird das Ergebnis um die Hälfte der eingegebenen Dicke auf beiden Seiten von der ursprünglichen Oberfläche versetzt.

Im folgenden Beispiel wird zunächst mit `Surface.BySweep2Rails` eine Oberfläche erstellt. Anschließend erstellen wir mithilfe eines Zahlen-Schiebereglers einen Volumenkörper, um die `thickness`-Eingabe eines `Surface.Thicken`-Blocks zu bestimmen. Ein boolescher Schalter steuert, ob die Verdickung auf beiden Seiten oder nur auf einer Seite erfolgen soll.

___
## Beispieldatei

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
