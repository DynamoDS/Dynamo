<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## Description approfondie
`Surface.TrimWithEdgeLoops` ajuste la surface avec un ensemble d'une ou de plusieurs PolyCurves fermées qui doivent toutes se trouver sur la surface dans la tolérance spécifiée. Si un ou plusieurs orifices doivent être ajustés sur la surface d'entrée, il doit y avoir une boucle externe spécifiée pour la limite de contour de la surface et une boucle interne pour chaque orifice. Si la région entre la limite de contour de surface et le(s) orifice(s) doit être ajustée, seule la boucle pour chaque orifice doit être fournie. Pour une surface périodique sans boucle externe telle qu'une surface sphérique, la région ajustée peut être contrôlée en inversant la direction de la courbe de boucle.

La tolérance correspond à la tolérance utilisée pour déterminer si les extrémités de la courbe coïncident et si une courbe et une surface coïncidente. La tolérance fournie ne peut pas être inférieure aux tolérances utilisées dans la création des PolyCurves d'entrée. La valeur par défaut de 0,0 signifie que la tolérance la plus élevée utilisée lors de la création des PolyCurves d'entrée sera utilisée.

Dans l'exemple ci-dessous, deux boucles sont ajustées en dehors d'une surface, renvoyant deux nouvelles surfaces mises en surbrillance en bleu. Le curseur numérique ajuste la forme des nouvelles surfaces.

___
## Exemple de fichier

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
