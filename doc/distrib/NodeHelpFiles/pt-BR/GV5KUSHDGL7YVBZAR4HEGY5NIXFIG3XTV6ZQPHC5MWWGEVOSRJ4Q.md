## Em profundidade
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Defina os limites para as coordenadas x e y especificando os valores mínimo e máximo. Esses limites definem os limites dentro dos quais os pontos serão redistribuídos. Em seguida, selecione uma curva matemática entre as opções fornecidas, que inclui as curvas linear, senoidal, de cosseno, de ruído de Perlin, de Bézier, gaussiana, parabólica, de raiz quadrada e de potência. Use os pontos de controle interativos para ajustar a forma da curva selecionada, adaptando-a às suas necessidades específicas.

É possível bloquear a forma da curva usando o botão de bloqueio, o que impede modificações adicionais na curva. Além disso, é possível redefinir a forma para seu estado padrão usando o botão Redefinir dentro do nó.

Especifique o número de pontos a serem redistribuídos definindo a entrada Count. O nó calcula novas coordenadas x para o número especificado de pontos com base na curva selecionada e nos limites definidos. Os pontos são redistribuídos de forma que suas coordenadas x sigam a forma da curva ao longo do eixo y.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## Arquivo de exemplo


