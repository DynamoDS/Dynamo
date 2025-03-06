## Informacje szczegółowe
Węzeł `TSplineSurface.FlattenVertices(vertices, parallelPlane)` zmienia położenie punktów sterujących dla określonego zestawu wierzchołków przez wyrównanie ich z płaszczyzną podaną w pozycji danych wejściowych `parallelPlane`.

W poniższym przykładzie wierzchołki powierzchni płaszczyzny T-splajn zostają przemieszczone za pomocą węzłów `TsplineTopology.VertexByIndex` i `TSplineSurface.MoveVertices`. Powierzchnia zostaje następnie przekształcona na potrzeby umieszczenia jej z boku w celu zapewnienia lepszego podglądu i zostaje przekazana jako dane wejściowe do węzła `TSplineSurface.FlattenVertices(vertices, parallelPlane)`. Wynikiem jest nowa powierzchnia z wybranymi wierzchołkami leżącymi płasko na podanej płaszczyźnie.
___
## Plik przykładowy

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
