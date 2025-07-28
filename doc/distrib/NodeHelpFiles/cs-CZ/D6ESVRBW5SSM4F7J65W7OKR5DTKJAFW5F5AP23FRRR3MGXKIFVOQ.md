<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
Podobně jako uzel `TSplineSurface.UnweldEdges` provede i tento uzel operaci zrušení svaru u sady vrcholů. V důsledku toho budou všechny hrany, které se spojují ve vybraném vrcholu, zrušeny. Na rozdíl od operace Zrušit vyostření, která vytvoří ostrý přechod kolem vrcholu, přičemž zachová propojení, operace Zrušit svar vytvoří nespojitost.

V níže uvedeném příkladu je u jednoho z vybraných vrcholů roviny T-Spline zrušen svar pomocí uzlu `TSplineSurface.UnweldVertices`. Podél hran obklopujících vybraný vrchol je zavedena nespojitost, která je znázorněna tažením vrcholu nahoru pomocí uzlu `TSplineSurface.MoveVertices`.

## Vzorový soubor

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
