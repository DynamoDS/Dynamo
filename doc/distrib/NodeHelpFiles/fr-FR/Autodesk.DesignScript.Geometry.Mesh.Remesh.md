## Détails
'Mesh.Remesh' crée un nouveau maillage dans lequel les triangles d'un objet donné sont redistribués de façon plus égale quelle que soit la modification des normales des triangles. Cette opération peut être utile pour les maillages ayant une densité variable de triangles afin de préparer le maillage pour l'analyse de la force. Le remaillage répété d'un maillage génère des maillages de plus en plus uniformes. Pour les maillages dont les sommets sont déjà équidistants (par exemple, un maillage d’icosphère), le résultat du noeud 'Mesh.Remesh' est le même maillage.
Dans l'exemple ci-dessous, 'Mesh.Remesh' est utilisé sur un maillage importé avec une forte densité de triangles dans des zones avec un niveau de détail élevé. Le résultat du noeud 'Mesh.Remesh' est transposé sur le côté et 'Mesh.Edges' est utilisé pour visualiser le résultat.

'(Le fichier d'exemple utilisé est sous licence Creative Commons)'

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.Remesh_img.jpg)
