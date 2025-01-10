## Em profundidade
Repair tentará reparar sólidos que tenham geometria inválida, bem como executar otimizações. O nó Repair retornará um novo objeto sólido.
Esse nó é útil quando você encontra erros ao executar operações em geometria importada ou convertida.

Por exemplo, se você importar dados de um contexto de host como **Revit** ou de um arquivo **.SAT** e descobrir que ele não é aparado ou boleano, poderá descobrir que uma operação de reparo limpa qualquer *geometria inválida* que esteja causando a falha.

Em geral, você não deve precisar usar essa funcionalidade na geometria que criar no Dynamo, somente na geometria de fontes externas. Se achar que não é o caso, relate um erro no github da equipe do Dynamo.
___


