## In-Depth
V níže uvedeném příkladu je povrch T-Spline vytvořen jako vysunutí daného profilu ve vstupu `curve`. Křivka může být otevřená nebo uzavřená. Vysunutí je prováděno ve směru daném vstupem `direction`, může být provedeno v obou směrech a je řízeno vstupy `frontDistance` a `backDistance`. Pomocí daných hodnot `frontSpans` a `backSpans` je možné samostatně nastavit rozpětí pro oba směry vysunutí. Vstup `profileSpans` určuje za účelem stanovení definice povrchu podél křivky počet ploch a vstup `uniform` je buď rovnoměrně rozloží, nebo vezme v úvahu zakřivení. Nakonec vstup `inSmoothMode` určuje, zda je povrch zobrazen v režimu vyhlazení nebo kvádru.

## Vzorový soubor
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
