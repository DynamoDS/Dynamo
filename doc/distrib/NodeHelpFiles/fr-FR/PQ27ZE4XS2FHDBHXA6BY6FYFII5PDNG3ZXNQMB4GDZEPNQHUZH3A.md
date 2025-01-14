<!--- Autodesk.DesignScript.Geometry.Surface.BySweep(profile, path, cutEndOff) --->
<!--- PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A --->
## Description approfondie
`Surface.BySweep (profile, path, cutEndOff)` crée une surface en balayant une courbe d'entrée le long d'une trajectoire spécifiée. L'entrée `cutEndOff` détermine s'il faut couper ou non l'extrémité du balayage et la rendre normale à la trajectoire.

Dans l'exemple ci-dessous, nous utilisons une courbe sinusoïdale dans la direction Y comme courbe de profil. Nous faisons pivoter cette courbe de -90 degrés autour de l'axe Z universel pour l'utiliser comme courbe de trajectoire. Le noeud "Surface BySweep" déplace la courbe de profil le long de la courbe de trajectoire pour créer une surface.


___
## Exemple de fichier

![Surface.BySweep](./PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A_img.jpg)
