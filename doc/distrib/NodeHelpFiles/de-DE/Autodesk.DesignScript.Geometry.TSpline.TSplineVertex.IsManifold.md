## In-Depth
Im folgenden Beispiel wird eine nicht mannigfaltige Oberfläche erstellt, indem zwei Oberflächen verbunden werden, die eine gemeinsame interne Kante aufweisen. Das Ergebnis ist eine Oberfläche ohne klare Vorder- und Rückseite. Die nicht mannigfaltige Oberfläche kann bis zur Reparatur nur im Modus Quader angezeigt werden. `TSplineTopology.DecomposedVertices` wird verwendet, um alle Scheitelpunkte der Oberfläche abzufragen, und der Block `TSplineVertex.IsManifold` wird verwendet, um hervorzuheben, welche Scheitelpunkte als mannigfaltig eingestuft werden. Die nicht mannigfaltigen Scheitelpunkte werden extrahiert, und ihre Position wird mithilfe der Blöcke `TSplineVertex.UVNFrame` und `TSplineUVNFrame.Position` visualisiert.


## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)
