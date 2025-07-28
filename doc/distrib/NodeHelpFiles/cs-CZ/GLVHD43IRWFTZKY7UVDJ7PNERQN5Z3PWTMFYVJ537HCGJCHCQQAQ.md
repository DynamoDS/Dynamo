<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
Uzel `TSplineReflection.SegmentsCount` vrací počet segmentů radiálního odrazu. Pokud je typ objektu TSplineReflection nastaven na hodnotu Osový, uzel vrací hodnotu 0.

V níže uvedeném příkladu se vytvoří povrch T-Spline s přidanými odrazy. Později v grafu je povrch dotazován uzlem `TSplineSurface.Reflections`. Výsledek (odraz) se poté použije jako vstup pro uzel `TSplineReflection.SegmentsCount` a vrátí se počet segmentů radiálního odrazu, který se použil k vytvoření povrchu T-Spline.

## Vzorový soubor

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
