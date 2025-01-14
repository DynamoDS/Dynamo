## Détails
L'opération 'Mesh.MakeHollow' peut être utilisée pour évider un objet maillé en préparation d'une iimpression 3D. L'évidement d'un maillage peut réduire considérablement la quantité de matériau d'impression nécessaire, le temps d'impression et le coût. L'entrée 'wallThickness' définit l'épaisseur des parois de l'objet maillé. En option, 'Mesh.MakeHollow' peut générer des trous d'évacuation pour retirer l'excès de matériau pendant le processus d'impression. La taille et le nombre de trous sont contrôlés par les entrées 'holeCount' et 'holeRadius'. Enfin, les entrées de 'meshResolution' et 'solidResolution' affectent la résolution du résultat du maillage. Une valeur 'meshResolution' plus élevée améliore la précision avec laquelle la partie intérieure du maillage décale le maillage d'origine, mais crée un plus grand nombre de triangles. Une valeur 'solidResolution' plus élevée améliore la mesure dans laquelle les détails plus fins du maillage d'origine sont conservés sur la partie intérieure du maillage évidé.
Dans l'exemple ci-dessous, 'Mesh.MakeHollow' est utilisé sur un maillage en forme de cône. Cinq trous d'évacuation sont ajoutés à sa base.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
