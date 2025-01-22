## Description approfondie
Dans l'exemple ci-dessous, les espaces dans une surface cylindrique de T-Spline sont remplis à l'aide du noeud `TSplineSurface.FillHole`, qui requiert les entrées suivantes :
- `edges` : nombre d'arêtes de bordure sélectionnées à partir de la surface de T-Spline à remplir
- `fillMethod` : valeur numérique comprise entre 0 et 3 indiquant la méthode de remplissage :
    * 0 remplit le trou avec un maillage par tesselation
    * 1 remplit le trou avec une seule face Polygone
    * 2 crée un point au centre du trou à partir duquel les faces triangulaires rayonnent vers les arêtes
    * 3 est similaire à la méthode 2, à la différence que les sommets du centre sont soudés en un sommet au lieu d'être simplement empilés.
- `keepSubdCreases` : valeur booléenne qui indique si les plis secondaires sont conservés.
___
## Exemple de fichier

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
