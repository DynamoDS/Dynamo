## In-Depth

Im folgenden Beispiel wird der Vorgang Schweißung aufheben für eine Reihe von Kanten einer T-Spline-Oberfläche durchgeführt. Die Verbindung der Scheitelpunkte der ausgewählten Kanten wird dadurch unterbrochen. Im Gegensatz zum Vorgang Knickstellen entfernen, bei dem ein scharfer Übergang um die Kante erstellt wird, während die Verbindung beibehalten wird, führt Schweißung aufheben zu einer Diskontinuität. Dies kann durch Vergleichen der Anzahl der Scheitelpunkte vor und nach dem Vorgang bewiesen werden. Alle nachfolgenden Vorgänge für nicht verschweißte Kanten oder Scheitelpunkte veranschaulichen ebenfalls, dass die Verbindung der Oberfläche entlang der nicht verschweißten Kante getrennt wird.

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
