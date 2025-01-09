<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## Em profundidade
`TSplineSurface.BuildFromLines` fornece uma forma para criar uma superfície da T-Spline mais complexa que pode ser usada como a geometria final ou como um primitivo personalizado que esteja mais próximo da forma desejada do que os primitivos padrão. O resultado pode ser uma superfície fechada ou aberta e pode ter furos e/ou arestas dobradas.

A entrada do nó é uma lista de curvas que representam uma “caixa de controle” para a superfície da T-Spline. Configurar a lista de linhas requer alguma preparação e deve seguir determinadas diretrizes.
- as linhas não devem se sobrepor
- a borda do polígono deve ser fechada e cada extremidade de linha deve encontrar pelo menos outro ponto final. Cada interseção de linha deve se encontrar em um ponto.
- uma densidade maior de polígonos é necessária para áreas com mais detalhes
- quadrados são preferíveis a triângulos e nGons porque são mais fáceis de controlar.

No exemplo abaixo, duas superfícies da T-Spline são criadas para ilustrar o uso desse nó. `maxFaceValence` é deixado no valor padrão para ambos os casos e `snappingTolerance` é ajustado para assegurar que as linhas dentro do valor de tolerância sejam tratadas como unidas. Para a forma à esquerda, `creaseOuterVertices` é definido como False para manter dois vértices de canto agudos e não arredondados. A forma à esquerda não apresenta vértices externos e essa entrada é deixada no valor padrão. `inSmoothMode` é ativado para ambas as formas para uma visualização suave.

___
## Arquivo de exemplo

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
