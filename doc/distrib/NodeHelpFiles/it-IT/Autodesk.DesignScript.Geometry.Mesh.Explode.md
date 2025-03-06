## In profondità
Il nodo `Mesh.Explode` utilizza una singola mesh e restituisce un elenco di facce della mesh come mesh indipendenti.

L'esempio seguente mostra una mesh cupola esplosa utilizzando `Mesh.Explode`, seguita da un offset di ciascuna faccia nella direzione della normale della faccia. Ciò si ottiene utilizzando i nodi `Mesh.TriangleNormals` e `Mesh.Translate`. Anche se in questo esempio le facce della mesh sembrano essere quadrilateri, in realtà sono triangoli con normali identiche.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
