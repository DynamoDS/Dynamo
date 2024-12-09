## Im Detail
Chamfer gibt einen neuen Volumenkörper mit gefasten Kanten zurück. Die Kanteneingabe gibt an, welche Kanten gefast werden sollen, während die Versatzeingabe das Ausmaß der Fase bestimmt. Im folgenden Beispiel beginnen Sie mit einem Würfel unter Verwendung der Vorgabeeingaben. Um die entsprechenden Kanten des Würfels zu erhalten, lösen Sie den Würfel zunächst auf, um die Flächen als Liste mit Oberflächen zu erhalten. Anschließend extrahieren Sie die Kanten des Würfels mit einem Face.Edges-Block. Extrahieren Sie die erste Kante jeder Fläche mit GetItemAtIndex. Ein Zahlen-Schieberegler steuert den Versatzabstand für die Fase.
___
## Beispieldatei

![Chamfer](./Autodesk.DesignScript.Geometry.Solid.Chamfer_img.jpg)

