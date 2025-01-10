## Description approfondie
Le mode Boîte et le mode Lissage sont deux moyens permettant de visualiser une surface de T-Spline. Le mode Lissage est la forme réelle d'une surface de T-Spline et est utile pour prévisualiser l'esthétique et les dimensions du modèle. Le mode Boîte, en revanche, permet d'obtenir une idée de la structure de la surface et une meilleure compréhension. Il s'agit également d'une option plus rapide pour prévisualiser une géométrie volumineuse ou complexe. Les modes Boîte et Lissage peuvent être contrôlés au moment de la création de la surface de T-Spline initiale ou ultérieure, avec des noeuds tels que `TSplineSurface.EnableSmoothMode`.

Si une T-Spline devient incorrecte, son aperçu passe automatiquement en mode boîte. Le noeud `TSplineSurface.IsInBoxMode` est un autre moyen d'identifier si la surface devient incorrecte.

Dans l'exemple ci-dessous, une surface plane de T-Spline est créée avec l'entrée `smoothMode` définie sur True. Deux de ses faces sont supprimées, ce qui rend la surface incorrecte. L'aperçu de la surface passe en mode boîte, bien qu'il soit impossible de le déterminer uniquement avec l'aperçu. Le noeud `TSplineSurface.IsInBoxMode` est utilisé pour confirmer que la surface est en mode boîte.
___
## Exemple de fichier

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
