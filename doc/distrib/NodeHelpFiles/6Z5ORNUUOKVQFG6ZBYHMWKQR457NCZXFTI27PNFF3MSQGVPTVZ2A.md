<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## In Depth
`PolyCurve.ByGroupedCurves` creates a new PolyCurve by grouping multiple connected curves. It groups curves based on their proximity to other curves, either touching or within a given join tolerance, to connect them into a single PolyCurve.

In the example below, a pentagon is exploded and its curves are randomized. `PolyCurve.ByGroupedCurves` is then used to group them into a PolyCurve.
___
## Example File

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)