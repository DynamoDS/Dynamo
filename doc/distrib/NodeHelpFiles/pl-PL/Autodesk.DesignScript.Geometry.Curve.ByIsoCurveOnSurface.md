## Informacje szczegółowe
Węzeł Curve by IsoCurve on Surface tworzy krzywą, która jest krzywą izometryczną na powierzchni, na podstawie kierunku U lub V i parametru w kierunku przeciwnym do tego, w którym należy utworzyć krzywą. Pozycja danych wejściowych „direction” określa kierunek tworzonej krzywej izometrycznej. Wartość jeden odpowiada kierunkowi U, a wartość zero — kierunkowi V. W poniższym przykładzie najpierw tworzymy siatkę punktów i przekształcamy je w kierunku Z o losową wartość. Punkty te są używane do utworzenia powierzchni za pomocą węzła NurbsSurface.ByPoints. Ta powierzchnia jest używana jako wejściowa baseSurface węzła ByIsoCurveOnSurface. Suwak Number Slider z ustawionym zakresem od 0 do 1 i krokiem 1 służy do określenia, czy należy wyodrębnić krzywą izometryczną w kierunku U, czy w kierunku V. Drugi suwak Number Slider służy do określenia parametru, przy którym krzywa izometryczna jest wyodrębniana.
___
## Plik przykładowy

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

