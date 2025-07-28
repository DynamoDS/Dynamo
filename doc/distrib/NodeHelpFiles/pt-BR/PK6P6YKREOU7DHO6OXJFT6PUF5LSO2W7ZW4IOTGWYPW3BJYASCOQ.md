<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` define se a geometria da T-Spline tem simetria radial. A simetria radial só pode ser introduzida para primitivos da T-Spline que permitam: cone, esfera, revolução, toroide. Depois de estabelecida na criação da geometria da T-Spline, a simetria radial influencia todas as operações e alterações subsequentes.

Um número desejado de `symmetricFaces` precisa ser definido para aplicar a simetria, sendo 1 o mínimo. Independentemente de quantos vãos de raios e alturas com os quais a superfície da T-Spline deve começar, ela será dividida em mais detalhes no número escolhido de `symmetricFaces`.

No exemplo abaixo, é criado o nó `TSplineSurface.ByConePointsRadii` e a simetria radial é aplicada por meio do uso do nó `TSplineInitialSymmetry.ByRadial`. Os nós `TSplineTopology.RegularFaces` e `TSplineSurface.ExtrudeFaces` são usados para respectivamente selecionar e efetuar a extrusão de uma face da superfície da T-Spline. A extrusão é aplicada simetricamente, e o controle deslizante para o número de faces simétricas demonstra como os vãos radiais são subdivididos.

## Arquivo de exemplo

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
