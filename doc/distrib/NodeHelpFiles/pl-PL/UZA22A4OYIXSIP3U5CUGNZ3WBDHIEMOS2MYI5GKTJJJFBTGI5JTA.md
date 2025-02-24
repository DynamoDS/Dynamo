<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## Informacje szczegółowe
W poniższym przykładzie dwie połówki powierzchni T-splajn zostają połączone w całość za pomocą węzła `TSplineSurface.ByCombinedTSplineSurfaces`. Wierzchołki wzdłuż płaszczyzny odbicia nakładają się na siebie, co staje się widoczne w przypadku przesunięcia jednego z wierzchołków za pomocą węzła `TSplineSurface.MoveVertices`. Aby to naprawić, wykonane zostaje złączenie za pomocą węzła `TSplineSurface.WeldCoincidentVertices`. Wynik przesunięcia wierzchołka jest teraz inny — zostaje on przekształcony na potrzeby umieszczenia go z boku w celu zapewnienia lepszego podglądu.
___
## Plik przykładowy

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
