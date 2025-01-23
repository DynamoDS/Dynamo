## Em profundidade
`Geometry.DeserializeFromSABWithUnits` importa a geometria para o Dynamo de uma matriz de bytes .SAB (Standard ACIS Binary) e de um `DynamoUnit.Unit` que é conversível de milímetros. Esse nó recebe um byte[] como a primeira entrada e um `dynamoUnit` como a segunda. Se a entrada `dynamoUnit` for deixada nula, isso importará a geometria .SAB como sem unidade, importando os dados geométricos na matriz sem qualquer conversão de unidade. Se uma unidade for fornecida, as unidades internas da matriz .SAB serão convertidas nas unidades especificadas.

O Dynamo é sem unidades, mas os valores numéricos no gráfico do Dynamo provavelmente ainda têm alguma unidade implícita. É possível usar a entrada `dynamoUnit` para dimensionar a geometria interna do .SAB para esse sistema de unidades.

No exemplo abaixo, um cuboide é gerado com base no SAB com duas unidades de medida (sem unidade). A entrada `dynamoUnit` dimensiona a unidade escolhida para uso em outro software.

___
## Arquivo de exemplo

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
