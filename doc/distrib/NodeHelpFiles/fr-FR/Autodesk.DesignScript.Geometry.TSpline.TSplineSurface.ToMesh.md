## Description approfondie
Dans l'exemple ci-dessous, une surface de boîte de T-Spline simple est transformée en maillage à l'aide d'un noeud `TSplineSurface.ToMesh`. L'entrée `minSegments` définit le nombre minimal de segments pour une face dans chaque direction et est importante pour contrôler la définition du maillage. L'entrée `tolerance` corrige les inexactitudes en ajoutant plus de positions de sommets pour correspondre à la surface d'origine dans la tolérance donnée. Le résultat est un maillage dont la définition est prévisualisée à l'aide d'un noeud `Mesh.VertexPositions`.
Le maillage de sortie peut contenir des triangles et des quadrilatères, ce qu'il faut garder à l'esprit si vous utilisez des noeuds MeshToolkit.
___
## Exemple de fichier

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
