<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## Im Detail
`Surface.TrimWithEdgeLoops` stutzt die Oberfläche mit einer Sammlung aus einem oder mehreren geschlossenen PolyCurve-Objekten, die sich alle auf der Oberfläche innerhalb der angegebenen Toleranz befinden müssen. Wenn eines oder mehrere Löcher von der eingegebenen Oberfläche gestutzt werden müssen, müssen eine äußere Kontur für die Begrenzung der Oberfläche und eine innere Kontur für jedes Loch angegeben werden. Wenn der Bereich zwischen der Oberflächenbegrenzung und den Löchern gestutzt werden muss, sollte nur die Kontur für die einzelnen Löcher angegeben werden. Für eine periodische Oberfläche ohne äußere Kontur, wie z. B. eine kugelförmige Oberfläche, kann der gestutzte Bereich durch Umkehren der Richtung der Konturkurve gesteuert werden.

Die Toleranz bezieht sich auf die Toleranz, die bei der Entscheidung verwendet wird, ob die Kurvenenden sowie eine Kurve und eine Oberfläche koinzident sind. Die angegebene Toleranz darf nicht kleiner sein als die Toleranzen, die bei der Erstellung der eingegebenen PolyCurve-Objekte verwendet werden. Der Vorgabewert 0.0 bedeutet, dass die größte Toleranz angewendet wird, die bei der Erstellung der eingegebenen PolyCurve-Objekte verwendet wurde.

Im folgenden Beispiel werden zwei Konturen aus einer Oberfläche gestutzt, sodass zwei neue Oberflächen entstehen, die blau hervorgehoben werden. Der Zahlen-Schieberegler passt die Form der neuen Oberflächen an.

___
## Beispieldatei

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
