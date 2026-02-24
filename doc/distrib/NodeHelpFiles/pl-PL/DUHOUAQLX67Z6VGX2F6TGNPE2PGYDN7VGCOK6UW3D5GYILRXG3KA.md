<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## Informacje szczegółowe
Węzeł Curve.SweepAsSurface tworzy powierzchnię przez przeciągnięcie krzywej wejściowej wzdłuż określonej ścieżki. W poniższym przykładzie tworzymy krzywą do przeciągnięcia, tworząc za pomocą bloku kodu trzy punkty węzła Arc.ByThreePoints. Krzywa definiująca jest tworzona jako linia prosta wzdłuż osi X. Węzeł Curve.SweepAsSurface przesuwa krzywą profilu wzdłuż krzywej ścieżki, tworząc powierzchnię. Parametr cutEndOff jest wartością logiczną, która steruje obróbką końcową powierzchni przeciągnięcia. Gdy jest w nim ustawiona wartość „true, końce powierzchni są przycinane prostopadle (jako normalne) do krzywej ścieżki, przez co powstają czyste, płaskie zakończenia. W przypadku ustawienia wartości „false” (domyślnej) końce powierzchni są zgodne z naturalnym kształtem krzywej profilu bez przycinania, co może spowodować powstanie końców pod kątem lub nierównych w zależności od krzywizny ścieżki.
___
## Plik przykładowy

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

