## Description approfondie
`Curve.OffsetMany` crée une ou plusieurs courbes en décalant une courbe plane selon la distance donnée dans un plan défini par la normale du plan. Si des espaces sont présents entre les courbes de composant décalées, ils sont remplis par la prolongation des courbes décalées.

L'entrée `planeNormal` est définie par défaut sur la normale du plan contenant la courbe, mais une normale explicite parallèle à la normale de la courbe d'origine peut être fournie pour mieux contrôler la direction du décalage.

Par exemple, si une direction de décalage cohérente est requise pour plusieurs courbes partageant le même plan, l'entrée `planeNormal` peut être utilisée pour remplacer les normales de courbe individuelles et forcer toutes les courbes à être décalées dans la même direction. L'inversion de la normale inverse la direction du décalage.

Dans l'exemple ci-dessous, une PolyCurve est décalée selon une distance de décalage négative, qui s'applique dans la direction opposée du produit cartésien entre la tangente de la courbe et le vecteur normal du plan.
___
## Exemple de fichier

![Curve.OffsetMany](./Autodesk.DesignScript.Geometry.Curve.OffsetMany_img.jpg)
