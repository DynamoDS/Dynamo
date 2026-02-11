<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## Em profundidade
No exemplo abaixo, duas metades de uma superfície da T-Spline são unidas em uma usando o nó `TSplineSurface.ByCombinedTSplineSurfaces`. Os vértices ao longo do plano de espelho são sobrepostos, os quais se tornam visíveis quando um dos vértices é movido usando o nó `TSplineSurface.MoveVertices`. Para corrigir isso, a soldagem é executada usando o nó `TSplineSurface.WeldCoincidentVertices`. O resultado da movimentação de um vértice agora é diferente, convertido na lateral para obter uma melhor visualização.
___
## Arquivo de exemplo

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
