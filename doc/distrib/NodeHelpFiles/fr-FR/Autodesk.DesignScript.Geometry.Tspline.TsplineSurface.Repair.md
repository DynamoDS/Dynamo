## Description approfondie
Dans l'exemple ci-dessous, une surface de T-Spline devient incorrecte, ce qui est visible en observant les faces se chevauchant dans l'aperçu en arrière-plan. Le fait que la surface soit incorrecte peut être confirmé par l'échec de l'activation du mode de lissage via le noeud `TSplineSurface.EnableSmoothMode`. Un autre indice est le noeud `TSplineSurface.IsInBoxMode` qui renvoie `true`, même si la surface a initialement activé le mode lisse.

Pour réparer la surface, elle est passée par un noeud `TSplineSurface.Repair`. Le résultat est une surface valide, qui peut être confirmée en activant avec succès le mode d'aperçu de lissage.
___
## Exemple de fichier

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
