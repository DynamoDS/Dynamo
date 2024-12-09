## Em profundidade
ReplaceByCondition considerará uma determinada lista e avaliará cada item com uma condição dada. Se a condição for avaliada como `true`, o item correspondente será substituído na lista de saída pelo item especificado na entrada replaceWith. No exemplo abaixo, vamos usar um nó Formula e inserir a fórmula x%2==0, que localiza o resto de um determinado item após dividir por 2, e verificar se o resto é igual a zero. Essa fórmula retornará `true` no caso de inteiros pares. Observe que o x de entrada é deixado em branco. Usar essa fórmula como a condição em um nó ReplaceByCondition resulta em uma lista de saída na qual cada número par é substituído pelo item especificado. Neste caso, o número inteiro 10.
___
## Arquivo de exemplo

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

