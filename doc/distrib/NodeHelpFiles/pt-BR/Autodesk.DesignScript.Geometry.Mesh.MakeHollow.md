## Em profundidade
É possível usar a operação 'Mesh.MakeHollow' para esvaziar um objeto de malha em preparação para impressão 3D. Esvaziar uma malha pode reduzir significativamente a quantidade de material de impressão necessário, o tempo e o custo de impressão. A entrada 'wallThickness' define a espessura das paredes do objeto de malha. Opcionalmente, 'Mesh.MakeHollow' pode gerar furos de escape para remover o excesso de material durante o processo de impressão. Os tamanhos e o número de furos são controlados pelas entradas 'holeCount' e 'holeRadius'. Por fim, as entradas para 'meshResolution' e 'solidResolution' afetam a resolução do resultado da malha. Uma 'meshResolution' maior melhora a precisão com a qual a parte interna da malha desloca a malha original, mas resultará em mais triângulos. Uma 'solidResolution' maior melhora a extensão em que detalhes mais finos da malha original são preservados na parte interna da malha oca.
No exemplo abaixo, 'Mesh.MakeHollow' é usado em uma malha na forma de um cone. Cinco furos de escape são adicionados em sua base.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
