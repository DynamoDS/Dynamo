## Im Detail
`Mesh.MakeWatertight` generiert ein festes, wasserdichtes und 3D-druckbares Netz durch Sampling des ursprünglichen Netzes. Dies ist eine schnelle Möglichkeit zum Auflösen eines Netzes mit zahlreichen Problemen wie Selbstüberschneidungen, Überlappungen und nicht mannigfaltiger Geometrie. Die Methode berechnet ein Dünnband-Abstandsfeld und generiert mithilfe des Marching-Cubes-Algorithmus ein neues Netz, projiziert jedoch nicht zurück auf das ursprüngliche Netz. Sie eignet sich eher für Netzobjekte mit zahlreichen Defekten oder schwierigen Problemen wie Selbstüberschneidungen.
Das folgende Beispiel zeigt eine nicht wasserdichte Vase und ihr wasserdichtes Pendant.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeWatertight_img.jpg)
