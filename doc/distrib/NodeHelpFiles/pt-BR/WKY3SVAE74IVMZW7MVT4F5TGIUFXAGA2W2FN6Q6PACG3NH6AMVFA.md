<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## Em profundidade
No exemplo abaixo, uma superfície da T-Spline é gerada através do nó `TSplineSurface.ByBoxLengths`.
Uma face é selecionada usando o nó `TSplineTopology.FaceByIndex` e é subdividida usando o nó `TSplineSurface.SubdivideFaces`.
Esse nó divide as faces especificadas em faces menores – quatro para faces regulares, três, cinco ou mais para NGons.
Quando a entrada booleana para `exact` estiver definida como True, o resultado será uma superfície que tenta manter exatamente a mesma forma que a original ao adicionar a subdivisão. Mais isocurvas podem ser adicionadas para preservar a forma. Quando definida como False, o nó subdividirá somente a face selecionada, o que geralmente resulta em uma superfície diferente da original.
Os nós `TSplineFace.UVNFrame` e `TSplineUVNFrame.Position` são usados para realçar o centro da face que está sendo subdividida.
___
## Arquivo de exemplo

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
