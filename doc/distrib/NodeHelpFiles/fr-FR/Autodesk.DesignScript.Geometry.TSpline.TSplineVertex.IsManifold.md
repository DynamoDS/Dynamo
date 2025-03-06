## In-Depth
Dans l'exemple ci-dessous, une surface non manifold est produite en joignant deux surfaces qui partagent une arête interne. Le résultat est une surface sans face et face arrière dégagée. La surface non-manifold peut uniquement être affichée en mode boîte jusqu'à ce qu'elle soit réparée. `TSplineTopology.DecomposedVertices` est utilisé pour interroger tous les sommets de la surface et le noeud `TSplineVertex.IsManifold` permet de mettre en surbrillance les sommets pouvant être considérés comme manifold. Les sommets non manifold sont extraits et leur position est visualisée à l'aide des noeuds `TSplineVertex.UVNFrame` et `TSplineUVNFrame.Position`.


## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)
