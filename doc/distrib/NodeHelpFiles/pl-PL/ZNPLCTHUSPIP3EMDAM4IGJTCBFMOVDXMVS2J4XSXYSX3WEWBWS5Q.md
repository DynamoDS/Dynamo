<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## Informacje szczegółowe
Węzeł Coordinate System At Segment Length zwraca układ współrzędnych wyrównany do krzywej wejściowej (curve) przy określonej długości krzywej, mierzonej od punktu początkowego krzywej. Wynikowy układ współrzędnych ma oś X w kierunku wektora normalnego krzywej oraz oś Y w kierunku stycznej krzywej na określonej długości. W poniższym przykładzie najpierw tworzymy krzywą Nurbs za pomocą węzła ByControlPoints na podstawie zestawu losowo wygenerowanych punktów. Suwak Number Slider steruje wejściową długością segmentu dla węzła CoordinateSystemAtParameter. Jeśli określona długość jest większa niż długość krzywej, węzeł zwróci układ współrzędnych w punkcie końcowym krzywej.
___
## Plik przykładowy

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

