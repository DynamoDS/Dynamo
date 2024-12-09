## Détails
'Mesh.ByGeometry' prend des objets de géométrie Dynamo (surfaces ou solides) comme entrée et les convertit en maillage. Les points et les courbes n'ont pas de représentations de maillage et ne sont donc pas des entrées valides. La résolution du maillage généré lors de la conversion est contrôlée par les deux entrées 'tolerance' et 'maxGridLines'. La 'tolérance' définit la déviation acceptable du maillage par rapport à la géométrie d'origine et est soumise à la taille du maillage. Si la valeur de 'tolerance' est définie sur -1, Dynamo choisit une tolérance raisonnable. L'entrée 'maxGridLines' définit le nombre maximal de lignes de grille dans la direction U ou V. Un nombre plus élevé de lignes de grille permet d'augmenter le lissage de la tesselation.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
