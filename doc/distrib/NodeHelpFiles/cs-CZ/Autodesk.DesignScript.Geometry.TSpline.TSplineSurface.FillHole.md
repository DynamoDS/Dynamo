## Podrobnosti
V níže uvedeném příkladu jsou mezery ve válcovém povrchu T-Spline vyplněny pomocí uzlu `TSplineSurface.FillHole`, který vyžaduje následující vstupy:
- `edges`: počet okrajových hran vybraných z povrchu T-Spline k vyplnění
- `fillMethod`: číselná hodnota od 0 do 3, která označuje metodu plnění:
    * 0 vyplní díru mozaikou.
    * 1 vyplní díru jednou plochou NGon
    * 2 vytvoří bod ve středu díry, ze kterého se budou šířit trojúhelníkové plochy směrem k hranám
    * 3 je podobná metodě 2, s rozdílem, že středové vrcholy jsou svařeny do jednoho vrcholu místo pouhého naskládání na sebe nahoře.
- `keepSubdCreases‘: booleovská hodnota, která udává, zda jsou zachována vyostření rozdělených částí.
___
## Vzorový soubor

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
