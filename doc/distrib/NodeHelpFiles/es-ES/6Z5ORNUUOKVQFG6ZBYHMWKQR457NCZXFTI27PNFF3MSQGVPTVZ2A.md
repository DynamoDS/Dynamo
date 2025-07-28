<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## En detalle:
`PolyCurve.ByGroupedCurves` crea una PolyCurve nueva mediante la agrupación de varias curvas conectadas. Agrupa curvas en función de su proximidad a otras curvas, ya sea tocándose o dentro de una tolerancia de unión determinada, para conectarlas en una única PolyCurve.

En el ejemplo siguiente, se descompone un pentágono y se organizan de forma aleatoria sus curvas. A continuación, se utiliza `PolyCurve.ByGroupedCurves` para agruparlas en una PolyCurve.
___
## Archivo de ejemplo

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
