<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## Podrobnosti
Uzel `TSplineSurface.BuildFromLines` umožňuje vytvořit složitější povrch T-Spline, který je možné použít buď jako konečnou geometrii nebo jako vlastní základní tvar, který je blíže požadovanému tvaru než výchozí základní útvary. Výsledkem může být buď uzavřený, nebo otevřený povrch a může obsahovat díry nebo vyostřené hrany tvaru.

Vstupem uzlu je seznam křivek, které představují‚ „řídicí klec“ pro povrch TSpline. Nastavení seznamu úseček vyžaduje určitou přípravu a musí se řídit určitými pokyny.
- čáry se nesmí překrývat
- hranice polygonu musí být uzavřená a každý koncový bod úsečky se musí setkat alespoň s jedním dalším koncovým bodem. Každý průsečík úsečky se musí nacházet v bodu.
- u oblastí s větším množstvím detailů je vyžadována větší hustota polygonů
- čtyřúhelníky jsou upřednostňovány před trojúhelníky a položkami nGon, protože čtyřúhelníky jsou snáze ovladatelné.

V níže uvedeném příkladu jsou vytvořeny dva povrchy T-Spline, které znázorňují použití tohoto uzlu. Vstup `maxFaceValence` je ponechán na výchozí hodnotě pro oba případy a vstup `snappingTolerance` je upraven tak, aby bylo zajištěno, že budou úsečky v rámci hodnoty tolerance považovány za propojené. U tvaru vlevo je vstup `creaseOuterVertices` nastaven na hodnotu False, aby byly dva rohové vrcholy ostré a ne zaoblené. Tvar nalevo neobsahuje žádné vnější vrcholy a tento vstup je ponechán na výchozí hodnotě. Uzel `inSmoothMode` se aktivuje u obou tvarů, aby byl zobrazený náhled vyhlazený.

___
## Vzorový soubor

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
