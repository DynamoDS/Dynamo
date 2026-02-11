## Em profundidade

O gráfico de barras cria um gráfico com barras orientadas verticalmente. As barras podem ser organizadas em vários grupos e rotuladas com códigos de cores. Você tem a opção de criar um único grupo inserindo um único valor duplo ou vários grupos inserindo vários valores duplos por sublista na entrada de valores. Para definir categorias, insira uma lista de valores de sequência de caracteres na entrada de legendas. Cada valor cria uma nova categoria codificada por cores.

Para atribuir um valor (altura) a cada barra, insira uma lista de listas contendo valores duplos na entrada de valores. Cada sublista determinará o número de barras e a qual categoria elas pertencem, na mesma ordem que as legendas inseridas. Se você tiver uma única lista de valores duplos, apenas uma categoria será criada. O número de valores de sequência de caracteres na entrada de legendas deve corresponder ao número de sublistas na entrada de valores.

Para atribuir uma cor para cada categoria, insira uma lista de cores na entrada de cores. Ao atribuir cores personalizadas, o número de cores deve corresponder ao número de valores de sequência de caracteres na entrada de legendas. Se nenhuma cor for atribuída, serão usadas cores aleatórias.

## Exemplo: grupo único

Imagine que você deseja representar a média de classificações de usuário para um item nos primeiros três meses do ano. Para visualizar isso, você precisa de uma lista de três valores de sequência de caracteres, rotulados Janeiro, fevereiro e março.
Portanto, para a entrada de legendas, forneceremos a seguinte lista em um bloco de código:

[“Janeiro”, “fevereiro”, “março”];

Também é possível usar os nós de sequência de caracteres conectados ao nó List.Create para criar a lista.

Em seguida, na entrada de valores, inseriremos a classificação média de usuário para cada um dos três meses como uma lista de listas:

[[3.5], [5], [4]];

Observe que, como temos três legendas, precisamos de três sublistas.

Agora, quando o gráfico for executado, o gráfico de barras será criado, com cada barra colorida representando a classificação média do cliente para o mês. Você pode continuar usando as cores padrão ou conectar uma lista de cores personalizadas na entrada de cores.

## Exemplo: vários grupos

É possível aproveitar a funcionalidade de agrupamento do nó do gráfico de barras, informando mais valores em cada sublista na entrada de valores. Neste exemplo, criaremos um gráfico visualizando o número de portas em três variações de três modelos, Modelo A, Modelo B e Modelo C.

Para fazer isso, primeiro forneceremos as legendas:

[“Modelo A”, “Modelo B”, “Modelo C”];

Em seguida, forneceremos valores, mais uma vez garantindo que o número de sublistas corresponda ao número de legendas:

[[17, 9, 13],[12,11,15],[15,8,17]];

Agora, quando você clicar em Executar, o nó do gráfico de barras criará um gráfico com três grupos de barras, marcados como Índice 0, 1 e 2, respectivamente. Neste exemplo, considere cada índice (ou seja, grupo) uma variação do projeto. Os valores no primeiro grupo (Índice 0) são extraídos do primeiro item em cada lista na entrada de valores, de modo que o primeiro grupo contém 17 para o Modelo A, 12 para o Modelo B e 15 para C. O segundo grupo (Índice 1) usa o segundo valor em cada grupo e assim por diante.

___
## Arquivo de exemplo

![Bar Chart](./CoreNodeModelsWpf.Charts.BarChartNodeModel_img.jpg)

