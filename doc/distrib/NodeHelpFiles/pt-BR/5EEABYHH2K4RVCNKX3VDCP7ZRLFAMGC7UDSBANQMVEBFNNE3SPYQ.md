<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## Em profundidade
`Curve.NormalAtParameter (curve, param)` retorna um vetor alinhado com a direção normal no parâmetro especificado de uma curva. A parametrização de uma curva é medida no intervalo de 0 a 1, com 0 representando o início da curva e 1 representando o final da curva.

No exemplo abaixo, primeiro criamos uma NurbsCurve usando um nó `NurbsCurve.ByControlPoints`, com um conjunto de pontos gerados aleatoriamente como entrada. Um controle deslizante numérico definido para o intervalo de 0 a 1 é usado para controlar a entrada `parameter` de um nó `Curve.NormalAtParameter`.
___
## Arquivo de exemplo

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
