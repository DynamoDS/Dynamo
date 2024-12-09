## Em profundidade

O nó Definir dados valida o tipo de dados de entrada. Ele pode ser usado para garantir que os dados locais sejam do tipo desejado e também foi projetado para ser usado como um nó de entrada ou saída, declarando o tipo de dados que um gráfico espera ou fornece. O nó oferece suporte a uma seleção de tipos de dados comumente usados do Dynamo, por exemplo, “Sequência de caracteres”, “Ponto” ou “Booleano”. A lista completa de tipos de dados com suporte está disponível no menu suspenso do nó. O nó oferece suporte a dados no formato de um único valor ou uma lista plana. Não há suporte para listas aninhadas, dicionários e replicação.

### Comportamento

O nó valida os dados provenientes da porta de entrada com base na configuração do menu suspenso Tipo e no botão de seleção Lista (veja abaixo para obter detalhes). Se a validação for bem-sucedida, a saída do nó será a mesma que a entrada. Se a validação não for bem-sucedida, o nó entrará em um estado de aviso com uma saída nula.
O nó tem uma entrada:

-   Entrada “**>**” – Conecta a um nó a montante para validar o tipo de seus dados.
    Além disso, o nó oferece três controles de usuário:
-   Botão de seleção ** Tipo de detecção automática** – Quando ativado, o nó analisa os dados de entrada e, se os dados forem de um tipo compatível, o nó definirá os controles de Tipo e Lista com base no tipo dos dados de entrada. O menu suspenso Tipo e o botão de seleção Lista estão desativados e serão atualizados com base no nó de entrada.

    Quando o tipo Detecção automática está desativado, é possível especificar um tipo de dados usando o menu Tipo e o botão de seleção Lista. Se os dados de entrada não corresponderem aos especificados, o nó entrará em um estado de aviso com uma saída nula.
-   Menu suspenso **Tipo** – Define o tipo de dados esperado. Quando o controle está ativado ( o botão de seleção **Tipo de detecção automática** está desativado), define um tipo de dados para a validação. Quando o controle está desativado (o botão de seleção **Tipo de detecção automática** está ativado), os tipos de dados são definidos automaticamente com base nos dados de entrada. Os dados serão válidos se seu tipo corresponder ao tipo mostrado exatamente ou se seu tipo for um filho do tipo mostrado (por exemplo, se o menu suspenso Tipo estiver definido como “Curva”, os objetos de tipo “Retângulo”, “Linha” etc. serão válidos).
- Botão de seleção **Lista** – Quando ativado, o nó espera que os dados de entrada sejam uma única lista plana contendo itens de um tipo de dados válido (consulte acima). Quando desativado, o nó espera um único item de um tipo de dados válido.

### Usar como nó de entrada

Quando definido como uma entrada (“É entrada” no menu de contexto do nó), o nó pode opcionalmente usar nós a montante para definir o valor padrão da entrada. Uma execução do gráfico armazenará em cache o valor do nó Definir dados para uso ao executar o gráfico externamente, por exemplo, com o nó Engine.

## Arquivo de exemplo

No exemplo abaixo, o primeiro grupo de nós “DefineData” tem o botão de seleção **Detecção automática do tipo**. O nó valida corretamente a entrada Número fornecida ao rejeitar a entrada Sequência de caracteres. O segundo grupo contém um nó com **Detecção automática do tipo** ativado. O nó ajusta automaticamente o menu suspenso Tipo e o botão de seleção Lista para corresponder à entrada; neste caso, uma lista de números inteiros.

![Define_Data](./CoreNodeModels.DefineData_img.png)
