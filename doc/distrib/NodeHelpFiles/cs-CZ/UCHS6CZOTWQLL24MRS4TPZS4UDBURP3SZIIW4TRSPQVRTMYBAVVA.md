<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
V níže uvedeném příkladu se uzel `TSplineSurface.UncreaseVertices` použije na rohové vrcholy základní roviny. Ve výchozím nastavení jsou tyto vrcholy vyostřeny v okamžiku vytvoření povrchu. Vrcholy jsou identifikovány pomocí uzlů `TSplineVertex.UVNFrame` a `TSplineUVNFrame.Poision` s aktivovanou možností `Show Labels`. Poté jsou vybrány rohové vrcholy pomocí uzlu `TSplineTopology.VertexByIndex` a jsou zbaveny vyostření. Pokud je tvar v režimu náhledu vyhlazení, je možné zobrazit náhled účinku této akce.

## Vzorový soubor

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
