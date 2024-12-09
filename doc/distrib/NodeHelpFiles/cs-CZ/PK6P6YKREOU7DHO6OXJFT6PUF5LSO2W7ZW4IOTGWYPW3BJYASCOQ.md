<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
Uzel `TSplineInitialSymmetry.ByRadial` definuje, zda má geometrie T-Spline radiální symetrii. Radiální symetrii je možné zavést pouze u základních tvarů T-Spline, které ji umožňují – kužel, koule, rotace a anuloid. Jakmile je radiální symetrie stanovena při tvorbě geometrie T-Spline, ovlivní všechny následné operace a úpravy.

Aby bylo možné použít symetrii, je třeba definovat požadovaný počet symetrických ploch daný vstupem `symmetricFaces`, přičemž minimum je 1. Bez ohledu na to, kolika rozpětími poloměrů a výšek musí povrch T-Spline začínat, bude dále rozdělen na vybraný počet symetrických ploch daný vstupem `symmetricFaces'.

V níže uvedeném příkladu se vytvoří uzel `TSplineSurface.ByConePointsRadii` a radiální symetrie se použije v uzlu `TSplineInitialSymmetry.ByRadial`. Poté se použijí uzly `TSplineTopology.RegularFaces` a `TSplineSurface.ExtrudeFaces` k výběru a vysunutí plochy povrchu T-Spline. Vysunutí se použije symetricky a posuvník počtu symetrických ploch znázorňuje, jak jsou radiální rozpětí dále dělena.

## Vzorový soubor

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
