## In-Depth
No exemplo abaixo, é criado um primitivo de cone da T-Spline usando o nó `TSplineSurface.ByConePointsRadius`. A posição e a altura do cone são controladas pelas duas entradas de `startPoint` e `endPoint`. Somente o raio base pode ser ajustado com a entrada `radius` e o raio superior é sempre zero. `radialSpans` e `heightSpans` determinam os vãos radial e de altura. A simetria inicial da forma é especificada pela entrada `symmetry`. Se a simetria X ou Y estiver definida como True, o valor dos vãos radiais deverá ser um múltiplo de 4. Por fim, a entrada `inSmoothMode` é usada para alternar entre a visualização do modo suave e de caixa da superfície da T-Spline.

## Arquivo de exemplo

![Example](./GVO3NNSNHNAH3DJS5OR37DI2A457QGYX4BQGMHO4IGUUUHZV3HSQ_img.jpg)
