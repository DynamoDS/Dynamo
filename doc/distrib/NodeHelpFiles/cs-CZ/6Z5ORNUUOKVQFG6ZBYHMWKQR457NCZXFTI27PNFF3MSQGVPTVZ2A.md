<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## Podrobnosti
Uzel `PolyCurve.ByGroupedCurves` vytvoří nový objekt PolyCurve seskupením více propojených křivek. Seskupuje křivky podle jejich blízkosti k jiným křivkám, buď dotýkajících se, nebo v rámci dané tolerance spojení, aby je spojil do jednoho objektu PolyCurve.

V následujícím příkladu je rozložen pětiúhelník a jeho křivky jsou nahodně vytvořené. Poté se k seskupení těchto křivek do objektu PolyCurve použije uzel `PolyCurve.ByGroupedCurves`.
___
## Vzorový soubor

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
