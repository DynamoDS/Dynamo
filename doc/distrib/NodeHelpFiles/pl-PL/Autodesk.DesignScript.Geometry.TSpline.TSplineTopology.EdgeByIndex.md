## Informacje szczegółowe
W poniższym przykładzie zostaje utworzony prostopadłościan T-splajn za pomocą węzła `TSplineSurface.ByBoxLengths` z określonym początkiem, szerokością, długością, wysokością, rozpiętościami i symetrią.
Następnie za pomocą węzła `EdgeByIndex` zostaje wybrana krawędź z listy krawędzi w wygenerowanej powierzchni. Wybrana krawędź zostaje przesunięta wzdłuż sąsiednich krawędzi za pomocą węzła `TSplineSurface.SlideEdges`, a następnie zostaje to zrobione dla jej symetrycznych odpowiedników.
___
## Plik przykładowy

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
