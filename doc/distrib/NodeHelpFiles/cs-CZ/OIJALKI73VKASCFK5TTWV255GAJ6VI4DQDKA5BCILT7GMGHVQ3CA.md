<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.NonManifoldVertices --->
<!--- OIJALKI73VKASCFK5TTWV255GAJ6VI4DQDKA5BCILT7GMGHVQ3CA --->
## In-Depth
Uzel `TSplineTopology.NonManifoldVertices` se používá k identifikaci nerozložených vrcholů z modelu T-Spline. Nerozložený povrch T-Spline je možné zobrazit pouze v režimu kvádru.

V níže uvedeném příkladu se vytvoří nerozložený povrch T-Spline jako výsledek spojení dvou povrchů, které sdílí stejnou hranu. Pomocí uzlů `TSplineTopology.NonManifoldVertices` a `TSplineUVNFrame.Position` je možné zvýraznit vrcholy, které jsou nerozložené.

## Vzorový soubor

![Example](./OIJALKI73VKASCFK5TTWV255GAJ6VI4DQDKA5BCILT7GMGHVQ3CA_img.jpg)
