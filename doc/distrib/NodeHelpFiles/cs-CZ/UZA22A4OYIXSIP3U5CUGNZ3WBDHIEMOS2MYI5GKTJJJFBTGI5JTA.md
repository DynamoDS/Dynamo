<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## Podrobnosti
V níže uvedeném příkladu jsou dvě poloviny povrchu T-Spline spojeny do jednoho povrchu pomocí uzlu `TSplineSurface.ByCombinedTSplineSurfaces`. Vrcholy podél roviny zrcadlení se překrývají, což se zobrazí, jakmile se posune jeden z vrcholů pomocí uzlu `TSplineSurface.MoveVertices`. Za účelem opravy tohoto jevu se provede svařování pomocí uzlu `TSplineSurface.WeldCoincidentVertices`. Výsledek přesunutí vrcholu je nyní jiný a je přesunut na stranu kvůli lepšímu náhledu.
___
## Vzorový soubor

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
