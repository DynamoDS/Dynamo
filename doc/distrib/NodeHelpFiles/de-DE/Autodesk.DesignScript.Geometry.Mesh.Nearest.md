## Im Detail
`Mesh.Nearest` gibt einen Punkt auf dem eingegebenen Netz zurück, der am nächsten zum angegebenen Punkt liegt. Der zurückgegebene Punkt ist eine Projektion des Eingabepunkts auf das Netz, wobei der Normalenvektor für das Netz verwendet wird, der durch den Punkt verläuft und so zum nächstgelegenen Punkt führt.

Im folgenden Beispiel wird ein einfacher Anwendungsfall erstellt, um die Funktionsweise des Blocks zu veranschaulichen. Der Eingabepunkt befindet sich über einem kugelförmigen Netz, jedoch nicht direkt darauf. Der resultierende Punkt ist der Punkt, der am nächsten zum Netz liegt. Dies steht im Gegensatz zur Ausgabe des Blocks `Mesh.Project` (wobei derselbe Punkt und dasselbe Netz als Eingaben verwendet werden, zusammen mit einem Vektor in negativer Z-Richtung), bei der der resultierende Punkt direkt unterhalb des Eingabepunkts auf das Netz projiziert wird. `Line.ByStartAndEndPoint` wird verwendet, um die 'Bewegungsbahn' des projizierten Punkts auf das Netz anzuzeigen.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.Nearest_img.jpg)
