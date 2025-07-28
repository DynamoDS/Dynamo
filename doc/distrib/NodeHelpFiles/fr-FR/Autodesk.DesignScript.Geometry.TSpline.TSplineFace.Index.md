## In-Depth
`TSplineFace.Index` renvoie l'index de la face sur la surface de T-Spline. Notez que dans une topologie de surface de T-Spline, les index de face, d'arête et de sommet ne coïncident pas nécessairement avec le numéro de séquence de l'élément dans la liste. Utilisez le noeud `TSplineSurface.CompressIndices` pour résoudre ce problème.

Dans l'exemple ci-dessous, `TSplineFace.Index` est utilisé pour afficher les index de toutes les faces régulières d'une surface de T-Spline.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Index_img.jpg)
