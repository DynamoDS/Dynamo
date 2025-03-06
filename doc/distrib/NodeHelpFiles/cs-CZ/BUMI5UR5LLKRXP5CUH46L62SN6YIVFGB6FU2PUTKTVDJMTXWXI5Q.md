## Podrobnosti

V níže uvedeném příkladu je povrch T-Spline spárován s hranou povrchu BRep pomocí uzlu `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)`. Minimální vstup požadovaný uzlem je základní vstup `tSplineSurface`, sada hran povrchu zadaná vstupem`tsEdge` a hrana nebo seznam hran zadaný vstupem `brepEdges`. Parametry shody jsou řízeny následujícími vstupy:
- vstup `continuity` umožňuje nastavit typ spojitosti pro shodu. Vstup očekává hodnoty 0, 1 nebo 2 odpovídající spojitostem G0 - poloha, G1 - tečna a G2 - zakřivení.
- vstup `useArcLength` řídí možnosti typu zarovnání. Pokud je nastavena hodnota True, použije se typ zarovnání Délka oblouku. Toto zarovnání minimalizuje fyzickou vzdálenost mezi každým bodem povrchu T-Spline a odpovídajícím bodem na křivce. Pokud je zadána hodnota False, použije se typ zarovnání Parametrické – každý bod na povrchu T-Spline je porovnán s bodem srovnatelné parametrické vzdálenosti podél cílové křivky.
- pokud je vstup `useRefinement` nastaven na hodnotu True, přidá řídicí body k povrchu a pokusí se tak vytvořit shodu s cílovou položkou v rámci tolerance dané vstupem `refinementTolerance`.
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## Vzorový soubor

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
