## Informacje szczegółowe
W poniższym przykładzie zostaje utworzona prosta powierzchnia prostopadłościanu T-splajn i jedna z jej krawędzi zostaje wybrana przy użyciu węzła `TSplineTopology.EdgeByIndex`. Aby zapewnić lepsze zrozumienie położenia wybranego wierzchołka, zostaje on zwizualizowany za pomocą węzłów `TSplineEdge.UVNFrame` i `TSplineUVNFrame.Position`. Wybrana krawędź zostaje przekazana jako dane wejściowe do węzła `TSplineSurface.SlideEdges` razem z powierzchnią, do której należy. Pozycja danych wejściowych `amount` określa, jak daleko krawędź przesuwa się w kierunku sąsiednich krawędzi (jest to wyrażone w procentach). Pozycja danych wejściowych `roundness` steruje płaskością lub zaokrągleniem skosu. Efekt zaokrąglenia jest bardziej zrozumiały w trybie ramki. Następnie wynik operacji przesuwania zostaje przekształcony na potrzeby umieszczenia go z boku w celu zapewnienia podglądu.

___
## Plik przykładowy

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
