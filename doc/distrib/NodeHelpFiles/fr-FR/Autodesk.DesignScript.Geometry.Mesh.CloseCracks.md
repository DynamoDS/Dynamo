## Détails
'Mesh.CloseCracks' ferme les fissures d'un maillage en supprimant les limites internes d'un objet de maillage. Des limites internes peuvent apparaître naturellement à la suite d'opérations de modélisation de maillage. Les triangles peuvent être supprimés lors de cette opération si des arêtes dégénérées sont supprimées. Dans l’exemple ci-dessous, 'Mesh.CloseCracks' est utilisé sur un maillage importé. 'Mesh.VertexNormals' permet de visualiser les sommets qui se chevauchent. Une fois que le maillage d'origine est passé par Mesh.CloseCracks, le nombre d'arêtes est réduit, ce qui est également évident en comparant le nombre d'arêtes, à l'aide d'un noeud 'Mesh.EdgeCount'.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
