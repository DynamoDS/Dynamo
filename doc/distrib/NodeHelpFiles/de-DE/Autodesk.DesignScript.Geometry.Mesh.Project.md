## Im Detail
`Mesh.Project` gibt einen Punkt auf dem Eingabenetz zurück, bei dem es sich um eine Projektion des Eingabepunkts auf dem Netz in Richtung des angegebenen Vektors handelt. Damit der Block ordnungsgemäß funktioniert, muss eine vom Eingabepunkt aus gezeichnete Linie in Richtung des Eingabevektors das angegebene Netz schneiden.

Das Beispieldiagramm zeigt die Funktionsweise des Blocks an einem einfachen Anwendungsfall. Der Eingabepunkt befindet sich über einem kugelförmigen Netz, jedoch nicht direkt darauf. Der Punkt wird in Richtung des negativen Z-Achsenvektors projiziert. Der resultierende Punkt wird auf die Kugel projiziert und wird direkt unter dem Eingabepunkt angezeigt. Dies steht im Gegensatz zur Ausgabe des `Mesh.Nearest`-Blocks (mit demselben Punkt und demselben Netz als Eingaben), bei der der resultierende Punkt entlang des 'Normalenvektors' auf dem Netz liegt, der durch den Eingabepunkt (dem nächstgelegenen Punkt) verläuft. `Line.ByStartAndEndPoint` wird verwendet, um die 'Bewegungsbahn' des projizierten Punkts auf dem Netz anzuzeigen.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.Project_img.jpg)
