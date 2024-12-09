## In-Depth
Uzel `TSplineVertex.IsStarPoint` vrací hodnotu, která určuje, zda je vrchol cípem hvězdy.

Cípy hvězdy existují, pokud se spojí 3, 5 nebo více hran. Přirozeně se vyskytují v základním kvádru nebo základní čtyřúhelníkové kouli a nejčastěji se vytváří při vysunutí plochy T-Spline, odstranění plochy nebo provedení sloučení. Na rozdíl od běžných vrcholů a vrcholů bodu T nejsou cípy hvězdy řízeny obdélníkovými řádky řídicích bodů. Cípy hvězdy ztěžují ovládání oblasti kolem nich a mohou způsobit zkreslení, čili by se měly používat pouze tam, kde je to nutné. Mezi nesprávná umístění pro cíp hvězdy patří ostřejší části modelu, například vyostřené hrany, části, kde se podstatně změní zakřivení, nebo hrana otevřeného povrchu.

Cípy hvězdy také určují, jak bude křivka T-Spline převedena na reprezentaci hranice (BREP). Pokud je křivka T-Spline převedena na BREP, rozdělí se na samostatné povrchy v každém cípu hvězdy.

V níže uvedeném příkladu je pomocí uzlu `TSplineVertex.IsStarPoint` zadán dotaz, zda vrchol vybraný uzlem `TSplineTopology.VertexByIndex` je cípem hvězdy.


## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
