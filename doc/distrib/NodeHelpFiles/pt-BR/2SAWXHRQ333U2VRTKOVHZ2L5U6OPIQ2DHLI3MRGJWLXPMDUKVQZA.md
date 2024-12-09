<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## Em profundidade
Cria uma NurbsSurface com vértices de controle, nós, espessuras e graus U V especificados. Há diversas restrições sobre os dados que, se quebrados, causarão a falha da função e gerarão uma exceção. Grau: ambos os graus u- e v- devem ser >= 1 (spline linear no âmbito da peça) e menor que 26 (o grau máximo com base na B-spline compatível com ASM). Espessuras: todos os valores de espessura (se fornecidos) devem ser estritamente positivos. Espessuras menores que 1e-11 serão rejeitadas e causarão falha na função. Nós: ambos os vetores de nós devem ser sequências não decrescentes. A multiplicidade do nó interno não deve ser maior que o grau mais 1 no nó inicial/final e o grau em nós internos (isso permite que as superfícies com descontinuidades G1 a serem representadas). Observe que vetores de nós não fixados são compatíveis, mas serão convertidos em fixados, com as alterações correspondentes aplicadas ao ponto de controle/dados da espessura.
___
## Arquivo de exemplo



