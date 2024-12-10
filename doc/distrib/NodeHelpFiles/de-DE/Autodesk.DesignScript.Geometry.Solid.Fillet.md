## Im Detail
Fillet gibt einen neuen Volumenkörper mit abgerundeten Kanten zurück. Die Kanteneingabe gibt an, welche Kanten abgerundet werden sollen, während die Versatzeingabe den Radius der Abrundung bestimmt. Im folgenden Beispiel beginnen Sie mit einem Würfel unter Verwendung der Vorgabeeingaben. Um die entsprechenden Kanten des Würfels zu erhalten, lösen Sie den Würfel zunächst auf, um die Flächen als Liste von Oberflächen zu erhalten. Anschließend extrahieren Sie die Kanten des Würfels mithilfe eines Face.Edges-Blocks. Extrahieren Sie die erste Kante der einzelnen Flächen mit GetItemAtIndex. Ein Zahlen-Schieberegler steuert den Radius für jede Abrundung.
___
## Beispieldatei

![Fillet](./Autodesk.DesignScript.Geometry.Solid.Fillet_img.jpg)

