<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## Em profundidade
`PolyCurve.ByGroupedCurves` cria uma nova PolyCurve agrupando várias curvas conectadas. Ele agrupa curvas com base em sua proximidade com outras curvas, seja tocando ou dentro de uma determinada tolerância de junção, para conectá-las em uma única PolyCurve.

No exemplo abaixo, um pentágono é explodido e suas curvas são randomizadas. Em seguida, é usado `PolyCurve.ByGroupedCurves` para agrupá-las em uma PolyCurve.
___
## Arquivo de exemplo

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
