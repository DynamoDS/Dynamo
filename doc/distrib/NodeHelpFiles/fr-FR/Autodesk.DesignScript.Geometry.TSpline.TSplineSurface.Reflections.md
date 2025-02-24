## Description approfondie
Dans l'exemple ci-dessous, une surface de T-Spline avec réflexions ajoutées est recherchée à l'aide du noeud `TSplineSurface.Reflections`, renvoyant une liste de toutes les réflexions appliquées à la surface. Le résultat est une liste de deux réflexions. La même surface est ensuite passée par un noeud `TSplineSurface.RemoveReflections` et inspectée à nouveau. Cette fois, le noeud `TSplineSurface.Reflections` renvoie une erreur, car les réflexions ont été supprimées.
___
## Exemple de fichier

![TSplineSurface.Reflections](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Reflections_img.jpg)
