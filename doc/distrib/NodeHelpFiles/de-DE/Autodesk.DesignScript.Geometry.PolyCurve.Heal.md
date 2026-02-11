## Im Detail
`PolyCurve.Heal` gibt mithilfe eines sich selbst schneidenden PolyCurve-Objekts ein neues PolyCurve-Objekt zurück, das sich nicht selbst schneidet. Das eingegebene PolyCurve-Objekt darf nicht mehr als 3 Punkte aufweisen, an denen es sich selbst schneidet. Mit anderen Worten: Wenn ein einzelnes Segment des PolyCurve-Objekts auf mehr als 2 andere Segmente trifft oder diese schneidet, funktioniert die Korrektur nicht. Geben Sie ein `trimLength`-Objekt größer als 0 ein. Damit werden Endsegmente, die länger als `trimLength` sind, nicht gestutzt.

Im folgenden Beispiel wird ein sich selbst schneidendes PolyCurve-Objekt mit `PolyCurve.Heal` korrigiert.
___
## Beispieldatei

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
