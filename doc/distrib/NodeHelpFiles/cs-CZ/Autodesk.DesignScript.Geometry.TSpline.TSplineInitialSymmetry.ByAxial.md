## Podrobnosti
Uzel `TSplineInitialSymmetry.ByAxial` definuje, zda má geometrie T-Spline symetrii podél vybrané osy (x, y, z). Symetrie může nastat na jedné, dvou nebo všech třech osách. Jakmile je symetrie stanovena při tvorbě geometrie T-Spline, ovlivní všechny následné operace a úpravy.

V níže uvedeném příkladu se pomocí uzlu `TSplineSurface.ByBoxCorners` vytvoří povrch T-Spline. Mezi vstupy tohoto uzlu slouží uzel `TSplineInitialSymmetry.ByAxial` k definování počáteční symetrie v povrchu. Pomocí uzlů `TSplineTopology.RegularFaces` a `TSplineSurface.ExtrudeFaces` se poté vybere a vysune plocha povrchu T-Spline. Operace vysunutí je poté zrcadlena kolem os symetrie definovaných uzlem `TSplineInitialSymmetry.ByAxial`.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
