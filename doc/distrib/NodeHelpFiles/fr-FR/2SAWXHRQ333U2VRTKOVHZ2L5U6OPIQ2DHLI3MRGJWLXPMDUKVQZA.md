<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## Description approfondie
Crée une NurbsSurface avec les sommets de contrôle, les nœuds, les poids et les degrés U et V spécifiés. Il existe plusieurs restrictions sur les données qui, en cas de non-respect, entraînent l'échec de la fonction et émettent une exception. Degré : les degrés U et V doivent être >= 1 (spline linéaire par morceaux) et inférieurs à 26 (degré de base de spline B maximum pris en charge par ASM). Poids : toutes les valeurs de poids (si indiquées) doivent être strictement positives. Les poids inférieurs à 1e-11 seront rejetés et la fonction échouera. nœuds : les deux vecteurs de nœud doivent être des séquences non décroissantes. La multiplicité de nœuds intérieure ne doit pas être supérieure au degré plus 1 au niveau du nœud de début/fin ni au degré d'un nœud interne (cela permet aux surfaces avec des discontinuités G1 d'être représentées). Notez que les vecteurs de nœuds non bloqués sont pris en charge, mais seront convertis en vecteurs de nœuds bloqués, avec les modifications correspondantes appliquées aux données de point de contrôle ou de poids.
___
## Exemple de fichier



