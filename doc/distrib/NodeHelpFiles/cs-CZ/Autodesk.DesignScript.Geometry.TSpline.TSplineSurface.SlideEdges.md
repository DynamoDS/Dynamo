## Podrobnosti
V níže uvedeném příkladu je vytvořen jednoduchý povrch kvádru T-Spline a jedna z jeho hran je vybrána pomocí uzlu `TSplineTopology.EdgeByIndex`. Pro lepší pochopení pozice vybraného vrcholu je tato pozice vizualizována pomocí uzlů `TSplineEdge.UVNFrame` a `TSplineUVNFrame.Position`. Vybraná hrana je předána jako vstup pro uzel `TSplineSurface.SlideEdges` společně s povrchem, ke kterému patří. Vstup `amount` určuje, o kolik se hrana posune směrem k sousedním hranám, což je vyjádřeno v procentech. Vstup `roundness` řídí plochost nebo zaoblení zkosení. Účinek zaoblení je lépe srozumitelný v režimu kvádru. Výsledek operace posunutí je poté přesunut na stranu za účelem náhledu.

___
## Vzorový soubor

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
