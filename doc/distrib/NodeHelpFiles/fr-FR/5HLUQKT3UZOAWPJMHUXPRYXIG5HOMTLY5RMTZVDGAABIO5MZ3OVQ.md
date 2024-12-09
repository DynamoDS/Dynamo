<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## Description approfondie
`Surface.Thicken (surface, thickness, both_sides)` crée un solide en décalant une surface en fonction de l'entrée `thickness` et en fermant les extrémités pour refermer le solide. Ce noeud dispose d'une entrée supplémentaire pour spécifier s'il faut ou non épaissir les deux côtés. L'entrée `both_sides` prend une valeur booléenne : True pour épaissir les deux côtés et False pour épaissir un seul côté. Notez que le paramètre `thickness` définit l'épaisseur totale du solide final, de sorte que si l'entrée `both_sides` est définie sur True, le résultat sera décalé de la surface d'origine avec la moitié de l'épaisseur `thickness` des deux côtés.

Dans l'exemple ci-dessous, nous créons d'abord une surface à l'aide d'un noeud `Surface.BySweep2Rails`. Nous créons ensuite un solide en utilisant un curseur numérique pour déterminer l'entrée `thickness` d'un noeud `Surface.Thicken`. Une bascule booléenne détermine si l'épaisseur doit être appliquée sur les deux côtés ou un seul.

___
## Exemple de fichier

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
