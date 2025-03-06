## Im Detail
`Mesh.Cone` erstellt einen Netzkegel, dessen Basis an einem Eingabe-Ursprungspunkt zentriert ist, mit einem Eingabewert für die oberen und unteren Radien, die Höhe und einer Anzahl von `divisions`. Die Anzahl der `divisions` entspricht der Anzahl der Scheitelpunkte, die an der Ober- und Unterseite des Kegels erzeugt werden. Wenn die Anzahl der `divisions` 0 beträgt, verwendet Dynamo einen Vorgabewert. Die Anzahl der Unterteilungen entlang der Z-Achse ist immer gleich 5. Die `cap`-Eingabe verwendet einen `Boolean`-Wert, um zu steuern, ob der Kegel oben geschlossen ist.
Im folgenden Beispiel wird der Block `Mesh.Cone` verwendet, um ein Netz in Form eines Kegels mit 6 Unterteilungen zu erstellen, daher sind die Unter- und die Oberseite des Kegels Sechsecke. Der Block `Mesh.Triangles` wird verwendet, um die Verteilung von Netzdreiecken zu visualisieren.


## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
