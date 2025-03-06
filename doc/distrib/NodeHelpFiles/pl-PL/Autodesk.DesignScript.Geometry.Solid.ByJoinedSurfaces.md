## Informacje szczegółowe
Węzeł Solid by Joined Surfaces pobiera listę powierzchni i zwraca pojedynczą bryłę zdefiniowaną przez te powierzchnie. Powierzchnie muszą definiować powierzchnię zamkniętą. W poniższym przykładzie zaczynamy od okręgu jako geometrii bazowej. Okrąg jest zamykany w celu utworzenia powierzchni, a następnie powierzchnia jest przekształcana w kierunku osi Z. Potem wyciągamy okrąg w celu utworzenia boków. Za pomocą węzła List.Create tworzona jest lista zawierająca powierzchnie podstawy, boku i góry, a następnie za pomocą węzła ByJoinedSurfaces lista ta jest przekształcana w pojedynczą bryłę zamkniętą.
___
## Plik przykładowy

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

