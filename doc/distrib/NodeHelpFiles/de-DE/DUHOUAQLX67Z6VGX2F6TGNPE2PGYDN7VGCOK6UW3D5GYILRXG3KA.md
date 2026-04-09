<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## Im Detail
`Curve.SweepAsSurface` erstellt eine Oberfläche durch Sweeping einer Eingabekurve entlang eines angegebenen Pfads. Im folgenden Beispiel wird eine zu sweepende Kurve erstellt, indem mithilfe eines Codeblocks drei Punkte eines `Arc.ByThreePoints`-Blocks erstellt werden. Eine Pfadkurve wird als einfache Linie entlang der X-Achse erstellt. `Curve.SweepAsSurface` verschiebt die Profilkurve entlang der Pfadkurve, wodurch eine Oberfläche erstellt wird. Der `cutEndOff`-Parameter ist ein boolescher Wert, der die Endenbearbeitung der Sweep-Oberfläche steuert. Wenn `true` festgelegt wird, werden die Enden der Oberfläche lotrecht (Normale) zur Pfadkurve geschnitten, wodurch saubere, flache Enden erzielt werden. Wenn `false` (Vorgabe) eingestellt wird, folgen die Oberflächenenden der natürlichen Form der Profilkurve, ohne gestutzt zu werden. Dies kann zu winkeligen oder ungleichen Enden führen, abhängig von der Pfadkurve.
___
## Beispieldatei

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

