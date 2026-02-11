## In-Depth
Uzel `TSplineSurface.BuildPipes` vytvoří potrubní povrch T-Spline pomocí sítě křivek. Jednotlivá potrubí jsou považována za spojená, pokud jsou jejich koncové body v mezích maximální tolerance nastavené vstupem `snappingTolerance`. Výsledek tohoto uzlu je možné vyladit pomocí sady vstupů, které umožňují nastavit hodnoty pro všechna potrubí nebo jednotlivě, pokud je zadán seznam délkou shodný s počtem potrubí. Tímto způsobem je možné použít následující vstupy: `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` a `endPositions`.

V níže uvedeném příkladu jsou jako vstup pro uzel `TSplineSurface.BuildPipes` zadány tři křivky spojené v koncových bodech. V tomto případě je vstup `defaultRadius` jedinou hodnotou pro všechna tři potrubí, čímž ve výchozím nastavení definuje poloměr potrubí, pokud nejsou k dispozici počáteční a koncové poloměry.
Dále vstup `segmentsCount` nastaví tři různé hodnoty pro každé jednotlivé potrubí. Vstup je seznam tří hodnot, z nichž každá odpovídá potrubí.

Další úpravy budou k dispozici, pokud budou možnosti `autoHandleStart` a `autoHandleEnd` nastaveny na hodnotu False. Toto umožní řídit počáteční a koncové otočení každého potrubí (vstupy `startRotations` a `endRotations`) a také poloměry na konci a začátku každého potrubí určením vstupů `startRadii` a `endRadii`. Nakonec vstupy `startPositions` a `endPositions` umožní odsadit segmenty na začátku nebo na konci každé křivky. Tento vstup očekává hodnotu odpovídající parametru křivky, kde segmenty začínají nebo končí (hodnoty mezi 0 a 1).

## Vzorový soubor
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
