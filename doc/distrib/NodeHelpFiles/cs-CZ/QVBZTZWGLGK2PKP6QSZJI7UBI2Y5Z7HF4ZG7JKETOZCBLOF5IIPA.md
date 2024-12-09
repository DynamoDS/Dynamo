<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## Podrobnosti
Uzel `TSplineSurface.DuplicateFaces` vytvoří nový povrch T-Spline vytvořený pouze z vybraných zkopírovaných ploch.

V níže uvedeném příkladu je povrch T-Spline vytvořen pomocí uzlu `TSplineSurface.ByRevolve`, který používá jako profil křivku NURBS.
Poté se vybere sada ploch na povrchu pomocí uzlu `TSplineTopology.FaceByIndex`. Tyto plochy jsou duplikovány pomocí uzlu `TSplineSurface.DuplicateFaces` a výsledný povrch je posunut na stranu, aby byl lépe vizualizován.
___
## Vzorový soubor

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
