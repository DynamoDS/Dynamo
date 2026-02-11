## Podrobnosti
V níže uvedeném příkladu je vytvořen rovinný povrch T-Spline pomocí uzlu `TSplineSurface.ByPlaneOriginNormal` a sada jeho ploch je vybrána a dále rozdělena. Tyto plochy jsou poté symetricky vysunuty pomocí uzlu `TSplineSurface.ExtrudeFaces` s daným směrem (v tomto případě normálový vektor UVN ploch) a počtem rozpětí. Výsledné hrany jsou rozmístěny v určeném směru.
___
## Vzorový soubor

![TSplineSurface.ExtrudeFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFaces_img.jpg)
