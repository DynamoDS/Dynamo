## Informacje szczegółowe
Węzeł `TSplineInitialSymmetry.ByAxial` określa, czy geometria T-splajnu ma symetrię wzdłuż wybranej osi (x, y, z). Symetria może wystąpić wzdłuż jednej, dwóch lub wszystkich trzech osi. Po jej ustanowieniu przy tworzeniu geometrii T-splajn symetria wpływa na wszystkie kolejne operacje i zmiany.

W poniższym przykładzie za pomocą węzła `TSplineSurface.ByBoxCorners` zostaje utworzona powierzchnia T-splajn. Wśród danych wejściowych tego węzła znajduje się węzeł `TSplineInitialSymmetry.ByAxial` służący do zdefiniowania symetrii początkowej na powierzchni. Za pomocą węzłów `TSplineTopology.RegularFaces` i `TSplineSurface.ExtrudeFaces` zostaje najpierw wybrana, a potem wyciągnięta powierzchnia w powierzchni T-splajn. Operacja wyciągnięcia prostego jest następnie odbijana wokół osi symetrii zdefiniowanych za pomocą węzła `TSplineInitialSymmetry.ByAxial`.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
