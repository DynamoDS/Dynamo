## Em profundidade
`PolyCurve.Heal` usa uma PolyCurve com autointerseção e retorna uma nova PolyCurve que não se autointersecta. A PolyCurve de entrada não pode ter mais de três autointerseções. Em outras palavras, se qualquer segmento da PolyCurve encontrar ou cruzar mais de dois outros segmentos, a recuperação não funcionará. Insira um `trimLength` maior que 0 e os segmentos finais maiores que `trimLength` não serão cortados.

No exemplo abaixo, uma PolyCurve com autointerseção é corrigida usando `PolyCurve.Heal`.
___
## Arquivo de exemplo

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
