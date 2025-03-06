<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## Podrobnosti
V níže uvedeném příkladu je povrch T-Spline vygenerován pomocí uzlu `TSplineSurface.ByBoxLengths`.
Plocha je vybrána pomocí uzlu `TSplineTopology.FaceByIndex` a je dále rozdělena pomocí uzlu `TSplineSurface.SubdivideFaces`.
Tento uzel rozdělí určené plochy na menší plochy – čtyři pro běžné plochy, tři, pět nebo více pro NGon.
Pokud je booleovský vstup `exact` nastaven na hodnotu true, výsledkem je povrch, který se při přidávání úseku pokouší zachovat stejný tvar jako původní. K zachování tvaru je možné přidat další izokřivky. Pokud je vstup nastaven na hodnotu false, uzel rozdělí pouze jednu vybranou plochu, což často způsobí, že se povrch liší od původního povrchu.
Uzly `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` slouží ke zvýraznění středu dělené plochy.
___
## Vzorový soubor

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
