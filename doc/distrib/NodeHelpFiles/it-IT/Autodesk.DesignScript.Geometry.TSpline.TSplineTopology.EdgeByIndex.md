## In profondità
Nell'esempio seguente, viene creato un parallelepipedo T-Spline utilizzando il nodo `TSplineSurface.ByBoxLengths` con origine, larghezza, lunghezza, altezza, campate e simmetria specificate.
`EdgeByIndex` viene quindi utilizzato per selezionare un bordo dall'elenco di bordi nella superficie generata. Il bordo selezionato viene quindi fatto scorrere lungo i bordi adiacenti utilizzando `TSplineSurface.SlideEdges`, seguito dalle relative controparti simmetriche.
___
## File di esempio

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
