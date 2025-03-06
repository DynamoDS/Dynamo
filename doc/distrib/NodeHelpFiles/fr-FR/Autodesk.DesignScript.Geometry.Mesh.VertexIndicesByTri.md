## Détails
'Mesh.VertexIndicesByTri' renvoie une liste aplanie d'index de sommet correspondant à chaque triangle de maillage. Les index sont ordonnés par trois, et les regroupements d'index peuvent être facilement reconstruits à l'aide du noeud 'List.Chop' avec l'entrée 'lengths' de 3.

Dans l'exemple ci-dessous, un 'MeshToolkit.Mesh' avec 20 triangles est converti en 'Geometry.Mesh'. 'Mesh.VertexIndicesByTri' permet d'obtenir la liste des index qui est ensuite divisée en listes de trois à l'aide de 'List.Chop'. La structure de la liste est inversée à l'aide de 'List.Transpose' pour obtenir trois listes de niveau supérieur de 20 index correspondant aux points A, B et C dans chaque triangle de maillage. Le noeud 'IndexGroup.ByIndices' est utilisé pour créer des groupes d'index de trois index chacun. La liste structurée de 'IndexGroups' et la liste des sommets sont ensuite utilisées comme entrée pour 'Mesh.ByPointsFaceIndices' afin d'obtenir un maillage converti.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
