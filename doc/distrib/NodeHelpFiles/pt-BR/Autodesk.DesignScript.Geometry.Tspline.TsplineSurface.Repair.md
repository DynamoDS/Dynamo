## Em profundidade
No exemplo abaixo, uma superfície da T-Spline se torna inválida, o que pode ser observado notando faces sobrepostas na visualização do plano de fundo. O fato de que a superfície é inválida pode ser confirmado por falha ao ativar o modo suave usando o nó `TSplineSurface.EnableSmoothMode`. Outra pista é o nó `TSplineSurface.IsInBoxMode` retornando `true`, mesmo que a superfície tenha ativado inicialmente o modo suave.

Para reparar a superfície, ela é passada através de um nó `TSplineSurface.Repair`. O resultado é uma superfície válida, que pode ser confirmada ativando com êxito o modo de visualização suave.
___
## Arquivo de exemplo

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
