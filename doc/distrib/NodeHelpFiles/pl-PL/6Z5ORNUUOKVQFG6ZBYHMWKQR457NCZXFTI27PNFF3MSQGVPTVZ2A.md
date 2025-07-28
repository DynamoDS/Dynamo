<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## Informacje szczegółowe
Węzeł `PolyCurve.ByGroupedCurves` tworzy nową krzywą PolyCurve przez zgrupowanie wielu połączonych krzywych. Grupuje krzywe na podstawie ich bliskości do innych krzywych: aby krzywe zostały połączone w pojedynczą krzywą PolyCurve, muszą albo stykać się ze sobą, albo znajdować się w zakresie danej tolerancji łączenia.

W poniższym przykładzie pięciokąt zostaje rozbity, a jego krzywe zostają przetworzone losowo. Następnie zostają zgrupowane do postaci krzywej PolyCurve za pomocą węzła `PolyCurve.ByGroupedCurves`.
___
## Plik przykładowy

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
