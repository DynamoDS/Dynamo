## Em profundidade
No exemplo abaixo, é criada uma superfície da T-Spline varrendo um `perfil` em torno de um determinado `path`. A entrada `parallel` controla se os vãos do perfil permanecem paralelos à direção do caminho ou rotacionam ao longo dele. A definição da forma é estabelecida por `pathSpans` e `radialSpans`. A entrada `pathUniform` define se os vãos do caminho são distribuídos de forma uniforme ou se tomam em conta a curvatura. Uma configuração similar, `profileUniform`, controla os vãos ao longo do perfil. A simetria inicial da forma é especificada pela entrada `symmetry`. Por fim, a entrada `inSmoothMode` é usada para alternar entre a visualização suave e de caixa da superfície da T-Spline.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySweep_img.jpg)
