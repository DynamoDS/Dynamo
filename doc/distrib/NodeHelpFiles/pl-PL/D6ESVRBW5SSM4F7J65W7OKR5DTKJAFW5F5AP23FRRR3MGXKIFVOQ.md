<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
Ten węzeł, podobnie jak węzeł `TSplineSurface.UnweldEdges`, wykonuje operację usuwania złączenia na zestawie wierzchołków. W wyniku tego wszystkie krawędzie łączące się w wybranym wierzchołku zostają rozłączone. W przeciwieństwie do operacji usuwania fałdowania (Uncrease), która tworzy ostre przejście wokół wierzchołka przy zachowaniu połączenia, operacja usuwania złączenia tworzy nieciągłość.

W poniższym przykładzie jeden z wybranych wierzchołków płaszczyzny T-splajn zostaje rozłączony za pomocą węzła `TSplineSurface.UnweldVertices`. Wzdłuż krawędzi otaczających wybrany wierzchołek zostaje wprowadzona nieciągłość, co jest zilustrowane przy użyciu przeciągnięcia wierzchołka w górę za pomocą węzła `TSplineSurface.MoveVertices`.

## Plik przykładowy

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
