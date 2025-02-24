<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## Em profundidade
O nó `TSplineSurface.DuplicateFaces` cria uma nova superfície da T-Spline composta somente de faces copiadas selecionadas.

No exemplo abaixo, é criada uma superfície da T-Spline por meio de `TSplineSurface.ByRevolve`, usando uma curva NURBS como um perfil.
Um conjunto de faces na superfície é selecionado usando `TSplineTopology.FaceByIndex`. Essas faces são duplicadas usando `TSplineSurface.DuplicateFaces` e a superfície resultante é deslocada para o lado para oferecer uma melhor visualização.
___
## Arquivo de exemplo

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
