<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## Informacje szczegółowe
Węzeł `TSplineSurface.DuplicateFaces` tworzy nową powierzchnię T-splajn, wykonaną tylko z wybranych skopiowanych powierzchni.

W poniższym przykładzie zostaje utworzona powierzchnia T-splajn za pomocą węzła `TSplineSurface.ByRevolve` i przy użyciu krzywej NURBS jako profilu.
Następnie za pomocą węzła `TSplineTopology.FaceByIndex` zostaje wybrany zestaw powierzchni z tej powierzchni. Te powierzchnie zostają powielone przy użyciu węzła `TSplineSurface.DuplicateFaces`, a wynikowa powierzchnia zostaje przesunięta na bok w celu zapewnienia lepszej wizualizacji.
___
## Plik przykładowy

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
