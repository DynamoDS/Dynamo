## Im Detail
`TSplineSurface.Thicken(vector, softEdges)` verdickt eine T-Spline-Oberfläche, die durch den angegebenen Vektor geführt wird. Der Verdickungsvorgang dupliziert die Oberfläche in `vector`-Richtung und verbindet anschließend die beiden Oberflächen durch Verbinden ihrer Kanten. Die boolesche Eingabe `softEdges` steuert, ob die resultierenden Kanten geglättet (True) oder geknickt (False) werden.

Im folgenden Beispiel wird eine extrudierte T-Spline-Oberfläche mit dem Block `TSplineSurface.Thicken(vector, softEdges)` verdickt. Die resultierende Oberfläche wird zur besseren Visualisierung zur Seite verschoben.


___
## Beispieldatei

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
