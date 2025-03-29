## Em profundidade
O nó `Curve Mapper` aproveita as curvas matemáticas para redistribuir os pontos dentro de um intervalo definido. A redistribuição neste contexto significa reatribuir as coordenadas x para novas posições ao longo de uma curva especificada com base em suas coordenadas y. Essa técnica é particularmente valiosa para aplicações como projeto de fachada, estruturas de telhado paramétricas e outros cálculos de projeto em que padrões ou distribuições específicos são necessários.

Defina os limites para as coordenadas x e y especificando os valores mínimo e máximo. Esses limites definem os limites dentro dos quais os pontos serão redistribuídos. Em seguida, selecione uma curva matemática entre as opções fornecidas, que inclui as curvas linear, senoidal, de cosseno, de ruído de Perlin, de Bézier, gaussiana, parabólica, de raiz quadrada e de potência. Use os pontos de controle interativos para ajustar a forma da curva selecionada, adaptando-a às suas necessidades específicas.

É possível bloquear a forma da curva usando o botão de bloqueio, o que impede modificações adicionais na curva. Além disso, é possível redefinir a forma para seu estado padrão usando o botão Redefinir dentro do nó.

Especifique o número de pontos a serem redistribuídos definindo a entrada Count. O nó calcula novas coordenadas x para o número especificado de pontos com base na curva selecionada e nos limites definidos. Os pontos são redistribuídos de forma que suas coordenadas x sigam a forma da curva ao longo do eixo y.

Por exemplo, para redistribuir 80 pontos ao longo de uma curva senoidal, defina X mín. como 0, X máx. como 20, Y mín. como 0 e Y máx. como 10. Após selecionar a curva senoidal e ajustar sua forma conforme necessário, o nó `Curve Mapper` gera 80 pontos com coordenadas x que seguem o padrão da curva senoidal ao longo do eixo y de 0 a 10.




___
## Arquivo de exemplo

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
