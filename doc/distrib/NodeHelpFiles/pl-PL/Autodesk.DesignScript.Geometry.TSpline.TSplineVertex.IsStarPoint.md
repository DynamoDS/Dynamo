## In-Depth
Węzeł `TSplineVertex.IsStarPoint` zwraca informację o tym, czy wierzchołek jest punktem gwiazdowym.

Punkty gwiazdowe istnieją, gdy zbiegają się 3 krawędzie, 5 krawędzi lub więcej. Występują naturalnie w prymitywach prostopadłościanowych lub kuli kwadrantowej i są najczęściej tworzone podczas wyciągania powierzchni T-splajn, usuwania powierzchni lub wykonywania operacji scalania. W przeciwieństwie do wierzchołków zwykłych i wierzchołków punktów T punktami gwiazdowymi nie sterują prostokątne rzędy punktów sterujących. Punkty gwiazdowe utrudniają sterowanie obszarem wokół nich i mogą powodować tworzenie zniekształceń, dlatego należy ich używać tylko tam, gdzie jest to konieczne. Niewłaściwe położenia punktów gwiazdowych to między innymi ostrzejsze części modelu, takie jak pofałdowane krawędzie, części, w których krzywizna znacząco się zmienia, oraz krawędź powierzchni otwartej.

Punkty gwiazdowe określają również to, jak T-splajn zostanie przekształcony w reprezentację obwiedni (BREP). Gdy T-splajn zostanie przekształcony w reprezentację obwiedni, zostanie podzielony na oddzielne powierzchnie w każdym punkcie gwiazdowym.

W poniższym przykładzie węzeł `TSplineVertex.IsStarPoint` służy do zbadania tego, czy wierzchołek wybrany za pomocą węzła `TSplineTopology.VertexByIndex` jest punktem gwiazdowym.


## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
