## Description approfondie
`TSplineSurface.Thicken(vector, softEdges)` épaissit une surface de T-Spline guidée par le vecteur spécifié. L'opération d'épaississement duplique la surface dans la direction `vecteur`, puis relie les deux surfaces en joignant leurs arêtes. L'entrée booléenne `softEdges` détermine si les arêtes obtenues sont lissées (true) ou pliées (false).

Dans l'exemple ci-dessous, une surface extrudée de T-Spline est épaissie à l'aide du noeud `TSplineSurface.Thicken(vector, softEdges)`. La surface résultante est traduite sur le côté pour une meilleure visualisation.


___
## Exemple de fichier

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
