## Podrobnosti

V níže uvedeném příkladu je povrch T-Spline spárován s křivkou NURBS pomocí
Uzel `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)`. Minimální vstup vyžadovaný pro uzel
je základní vstup `tSplineSurface`, sada hran povrchu zadaná ve vstupu `tsEdge` a křivka nebo
seznam oblouků.
Následující vstupy určují parametry shody:
– vstup `continuity` umožňuje nastavit typ spojitosti pro shodu. Vstup očekává hodnoty 0, 1 nebo 2 odpovídající možnostem G0 – poloha, G1 – tečna a G2 – zakřivení. Pro porovnání povrchu s křivkou je však k dispozici pouze hodnota G0 (vstupní hodnota 0).
- vstup `useArcLength` určuje možnosti typu zarovnání. Pokud je nastaven na hodnotu True, použije se typ trasy Délka
oblouku. Tato trasa minimalizuje fyzickou vzdálenost mezi jednotlivými body povrchu T-Spline a
odpovídajícími body na křivce. Pokud je na vstupu zadána hodnota False, typ zarovnání je Parametrické -
každý bod na povrchu T-Spline je porovnán s bodem ve srovnatelné parametrické vzdálenosti podél
cílové křivky shody.
- pokud je vstup `useRefinement` nastaven na hodnotu True, přidá řídicí body k povrchu a pokusí se tak vytvořit shodu s cílovou položkou
v rámci dané hodnoty `refinementTolerance`
- vstup `numRefinementSteps` je maximální počet dělení základního povrchu T-Spline
přičemž se uzel pokouší dosáhnout hodnoty `refinementTolerance`. Pokud je parametr `useRefinement` nastaven na hodnotu False, budou hodnoty `numRefinementSteps` a `refinementTolerance` ignorovány.
- vstup `usePropagation` určuje, jak velká část povrchu je ovlivněna shodou. Pokud je nastaven na hodnotu False, povrch je ovlivněn minimálně. Pokud je nastaven na hodnotu True, bude povrch ovlivněn v rámci zadané vzdálenosti `widthOfPropagation`.
- vstup `scale` je měřítko tečnosti, které ovlivňuje výsledky spojitosti G1 a G2.
- vstup `flipSourceTargetAlignment` obrátí směr trasy.


## Vzorový soubor

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
