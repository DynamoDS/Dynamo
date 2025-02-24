<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## Description approfondie
Dans l'exemple ci-dessous, une surface de T-Spline est générée par le noeud `TSplineSurface.ByBoxLengths`.
Une face est sélectionnée à l'aide du noeud `TSplineTopology.FaceByIndex`, puis subdivisée à l'aide du noeud `TSplineSurface.SubdivideFaces`.
Ce noeud divise les faces spécifiées en faces plus petites : quatre pour les faces normales, trois, cinq ou plus pour les polygones.
Lorsque l'entrée booléenne pour `exact` est définie sur True, le résultat est une surface qui tente de conserver la même forme que l'original lors de l'ajout de la subdivision. D'autres isocourbes peuvent être ajoutées pour conserver la forme. Lorsque cette option est définie sur False, le noeud subdivise uniquement la face sélectionnée, ce qui produit souvent une surface distincte de l'original.
Les noeuds `TSplineFace.UVNFrame` et `TSplineUVNFrame.Position` sont utilisés pour mettre en surbrillance le centre de la face subdivisée.
___
## Exemple de fichier

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
