## Em profundidade
O nó `Curve Mapper` redistribui uma série de valores de entrada dentro de um intervalo definido e alavanca curvas matemáticas para mapeá-los ao longo de uma curva especificada. Mapeamento, neste contexto, significa que os valores são redistribuídos de forma que suas coordenadas X sigam o formato da curva ao longo do eixo Y. Essa técnica é particularmente valiosa para aplicações como projeto de fachada, estruturas de telhado paramétricas e outros cálculos de projeto onde distribuições ou padrões específicos são necessários.

Defina os limites para as coordenadas X definindo os valores mínimo e máximo. Esses limites definem os limites dentro dos quais os pontos serão redistribuídos. É possível fornecer uma única contagem para gerar uma série de valores uniformemente distribuídos ou uma série de valores existente, que serão distribuídos ao longo da direção X dentro do intervalo especificado e, em seguida, mapeados para a curva.

Selecione uma curva matemática nas opções fornecidas, que incluem curvas Linear, Senoidal, Cosseno, Ruído de Perlin, Bézier, Gaussiana, Parabólica, Raiz quadrada e Potência. Use pontos de controle interativos para ajustar o formato da curva selecionada, adaptando-a às suas necessidades específicas.

É possível bloquear a forma da curva usando o botão de bloqueio, evitando modificações posteriores na curva. Além disso, é possível edefinir a forma para seu estado padrão usando o botão de redefinição dentro do nó. Se você obtiver NaN ou Null como saídas, poderá encontrar mais detalhes [aqui](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues) sobre o motivo pelo qual você pode estar vendo isso.

Por exemplo, para redistribuir 80 pontos ao longo de uma curva senoidal dentro do intervalo de 0 a 20, defina Mín. como 0, Máx. como 20 e Valores como 80. Depois de selecionar a curva senoidal e ajustar sua forma conforme necessário, o nó `Curve Mapper` gera 80 pontos com coordenadas X que seguem o padrão da curva senoidal ao longo do eixo Y.

Para mapear os valores distribuídos de forma desigual ao longo de uma curva Gaussiana, defina o intervalo mínimo e máximo e forneça a série de valores. Após selecionar a curva Gaussiana e ajustar sua forma conforme necessário, o nó `Curve Mapper` redistribui a série de valores ao longo das coordenadas X usando o intervalo especificado e mapeia os valores ao longo do padrão da curva. Para obter documentação detalhada sobre como o nó funciona e como definir entradas, confira [esta postagem blog](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) com foco no Mapeador de curva.




___
## Arquivo de exemplo

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
