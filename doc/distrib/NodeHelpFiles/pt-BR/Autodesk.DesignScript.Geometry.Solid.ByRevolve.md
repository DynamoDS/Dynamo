## Em profundidade
`Solid.ByRevolve` cria uma superfície rotacionando uma determinada curva de perfil em torno de um eixo. O eixo é definido por um ponto `axisOrigin` e um vetor `axisDirection`. O ângulo inicial determina onde começar a superfície, medido em graus, e o `sweepAngle` determina a distância em torno do eixo para continuar a superfície.

No exemplo abaixo, usamos uma curva gerada com uma função de cosseno como curva de perfil e dois controles deslizantes de números para controlar `startAngle` e`sweepAngle`. `axisOrigin`e `axisDirection` são deixados com os valores padrão da origem universal e do eixo z universal para este exemplo.

___
## Arquivo de exemplo

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

