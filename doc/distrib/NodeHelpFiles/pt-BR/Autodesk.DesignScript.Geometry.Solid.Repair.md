## Em profundidade
`Solid.Repair` tenta reparar sólidos que têm geometria inválida, além de potencialmente realizar otimizações. `Solid.Repair` retornará um novo objeto sólido.

Esse nó é útil quando você encontra erros ao executar operações em geometria importada ou convertida.

No exemplo abaixo, `Solid.Repair`' é usado para reparar a geometria de um arquivo **. SAT**. Ocorre falha na geometria do arquivo em operações booleanas ou de apara, e `Solid.Repair` limpa qualquer *geometria inválida* que esteja causando a falha.

Em geral, você não deve precisar usar essa funcionalidade na geometria criada no Dynamo, somente na geometria de fontes externas. Se esse não for o caso, relate o erro no Github da equipe do Dynamo
___
## Arquivo de exemplo

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
