<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
Dans l'exemple ci-dessous, le noeud `TSplineSurface.UncreaseVertices` est utilisé sur les sommets d'angle d'une primitive de plan. Par défaut, ces sommets sont pliés au moment de la création de la surface. Les sommets sont identifiés à l'aide des noeuds `TSplineVertex.UVNFrame` et `TSplineUVNFrame.Position`, avec l'option `Afficher les étiquettes` activée. Les sommets des coins sont ensuite sélectionnés à l'aide du noeud `TSplineTopology.VertexByIndex` et dépliés. L'effet de cette action peut être prévisualisé si la forme est en mode d'aperçu lisse.

## Exemple de fichier

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
