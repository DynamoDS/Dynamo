## Détails
Renvoie un nouveau maillage avec les défauts suivants réparés:
- Petits composants: si le maillage contient des segments très petits (par rapport à la taille globale du maillage) et déconnectés, ils seront ignorés.
- Trous: les trous dans le maillage sont remplis.
- Régions non multiples: si un sommet est connecté à plus de deux arêtes *limite* ou si une arête est connectée à plus de deux triangles, le sommet/l'arête est alors non multiple. La boîte à outils de maillage supprime la géométrie jusqu'à ce que le maillage soit multiple.
Cette méthode vise à préserver autant que possible le maillage d'origine, contrairement à MakeWatertight, qui rééchantillonne le maillage.

Dans l'exemple ci-dessous, 'Mesh.Repair' est utilisé sur un maillage importé pour remplir le trou autour de l'oreille de lapin.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.Repair_img.jpg)
