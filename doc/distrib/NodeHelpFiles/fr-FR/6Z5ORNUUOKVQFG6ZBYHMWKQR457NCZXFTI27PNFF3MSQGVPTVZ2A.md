<!--- Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves(curves, joinTolerance, trimCurves, trimLength) --->
<!--- 6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A --->
## Description approfondie
`PolyCurve.ByGroupedCurves` crée une nouvelle PolyCurve en regroupant plusieurs courbes connectées. Ce noeud regroupe les courbes en fonction de leur proximité avec d'autres courbes, en contact ou dans une tolérance de raccord donnée, afin de les connecter en une seule PolyCurve.

Dans l'exemple ci-dessous, un pentagone est décomposé et ses courbes sont aléatoires. Le noeud `PolyCurve.ByGroupedCurves` est ensuite utilisé pour les regrouper en une PolyCurve.
___
## Exemple de fichier

![PolyCurve.ByGroupedCurves](./6Z5ORNUUOKVQFG6ZBYHMWKQR457NCZXFTI27PNFF3MSQGVPTVZ2A_img.jpg)
