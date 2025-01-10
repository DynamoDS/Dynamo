<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
`TSplineReflection.SegmentsCount` renvoie le nombre de segments d'une réflexion radiale. Si le type de TSplineReflection est Axial, le noeud renvoie la valeur 0.

Dans l'exemple ci-dessous, une surface de T-Spline est créée avec des réflexions ajoutées. Plus loin dans le graphique, la surface est interrogée avec le noeud `TSplineSurface.Reflections`. Le résultat (une réflexion) est ensuite utilisé comme entrée pour le noeud `TSplineReflection.SegmentsCount` afin de renvoyer le nombre de segments d'une réflexion radiale qui a été utilisée pour créer la surface de T-Spline.

## Exemple de fichier

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
