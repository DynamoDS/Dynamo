## Détails
Ce noeud renvoie un nouveau maillage lisse à l'aide d'un algorithme de lissage de cotangente qui n'étale pas les sommets par rapport à leur position d'origine et convient mieux pour préserver les fonctions et les arêtes. Une valeur de mise à l'échelle doit être entrée dans le noeud pour définir l'échelle spatiale du lissage. Les valeurs de mise à l'échelle peuvent être comprises entre 0,1 et 64,0. Des valeurs plus élevées entraînent un effet de lissage plus visible, ce qui donne l'apparence d'un maillage plus simple. Bien qu'il semble plus lisse et plus simple, le nouveau maillage a le même nombre de triangles, d'arêtes et de sommets que le maillage initial.

Dans l'exemple ci-dessous, 'Mesh.ImportFile' est utilisé pour importer un objet. 'Mesh.Smooth' est ensuite utilisé pour lisser l'objet, avec une échelle de lissage de 5. L'objet est ensuite transposé dans une autre position avec 'Mesh.Translate' pour un meilleur aperçu, et 'Mesh.TriangleCount' est utilisé pour suivre le nombre de triangles dans l'ancien et le nouveau maillage.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.Smooth_img.jpg)
