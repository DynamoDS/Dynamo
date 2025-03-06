## Détails
Le noeud 'Mesh.GenerateSupport' permet d'ajouter des supports à la géométrie de maillage d'entrée afin de la préparer pour l'impression3D. Les supports sont nécessaires pour imprimer correcement la géométrie avec des saillies afin de garantir une bonne adhérence des couches et d'éviter que le matériau ne s'affaisse pendant le processus d'impression. 'Mesh.GenerateSupport' détecte les saillies et génère automatiquement des supports de type arborescence qui consomment moins de matériau et peuvent être plus facilement supprimés, ayant moins de contact avec la surface imprimée. Dans les cas où aucune saillie n'est détectée, le résultat du nœud 'Mesh.GenerateSupport' est le même maillage, pivoté et dans une orientation optimale pour l'impression et converti dans le plan XY. La configuration des supports est contrôlée par les entrées:
- baseHeight définit l'épaisseur de la partie la plus basse du support: sa base
- baseDiameter contrôle la taille de la base du support
- l'entrée postDiameter contrôle la taille de chaque support en son milieu
- tipHeight et tipDiameter contrôlent la taille des supports à leur extrémité, en contact avec la surface imprimée
Dans l'exemple ci-dessous, le noeud 'Mesh.GenerateSupport' est utilisé pour ajouter des supports à un maillage en forme de lettre 'T'.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)
