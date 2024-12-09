## Im Detail
Im folgenden Beispiel wird eine einfache T-Spline-Quaderoberfläche mit einem `TSplineSurface.ToMesh`-Block in ein Netz umgewandelt. Die Eingabe `minSegments` definiert die Mindestanzahl von Segmenten für eine Fläche in jeder Richtung und ist wichtig zum Steuern der Netzdefinition. Die Eingabe `tolerance` korrigiert Ungenauigkeiten, indem weitere Scheitelpunktpositionen hinzugefügt werden, um der ursprünglichen Oberfläche innerhalb der angegebenen Toleranz zu entsprechen. Das Ergebnis ist ein Netz, dessen Definition mit einem `Mesh.VertexPositions`-Block als Vorschau angezeigt wird.
Das Ausgabenetz kann sowohl Dreiecke als auch Vierecke enthalten. Dies ist zu beachten, wenn Sie MeshToolkit-Blöcke verwenden.
___
## Beispieldatei

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
