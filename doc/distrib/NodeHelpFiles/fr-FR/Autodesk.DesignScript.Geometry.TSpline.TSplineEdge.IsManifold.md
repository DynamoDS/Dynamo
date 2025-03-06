## In-Depth
Dans l'exemple ci-dessous, pour illustrer le cas d'une arête non-manifold, une surface est créée en joignant deux surfaces qui partagent une arête interne. Une surface qui n'a pas d'avant et d'arrière clairement définie est obtenue. La surface non-manifold ne peut être affichée en mode boîte qu'une fois réparée. Le noeud `TSplineEdge.IsManifold` est utilisé dans ce cas pour mettre en surbrillance les arêtes internes et de bordures qui sont manifold.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsManifold_img.jpg)
