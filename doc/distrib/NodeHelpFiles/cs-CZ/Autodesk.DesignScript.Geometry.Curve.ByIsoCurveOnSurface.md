## Podrobnosti
Uzel Curve by IsoCurve on Surface vytvoří křivku, která je izokřivkou na povrchu, určením směru U nebo V a určením parametru v opačném směru, ve kterém se má křivka vytvořit. Vstup `direction` určuje, který směr izokřivky má být vytvořen. Jedničková hodnota odpovídá směru U, zatímco nulová hodnota odpovídá směru V. V níže uvedeném příkladu nejprve vytvoříme osnovu bodů a náhodně je posuneme ve směru Z. Pomocí těchto bodů a uzlu NurbsSurface.ByPoints se vytvoří povrch. Tento povrch se použije jako objekt baseSurface uzlu ByIsoCurveOnSurface. Pomocí číselného posuvníku nastaveného na rozsah od 0 do 1 a kroku o hodnotě 1 se určí, zda extrahujeme izokřivku ve směru u nebo ve směru v. Pomocí druhého číselného posuvníku se určí parametr, ve kterém bude izokřivka extrahována.
___
## Vzorový soubor

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

