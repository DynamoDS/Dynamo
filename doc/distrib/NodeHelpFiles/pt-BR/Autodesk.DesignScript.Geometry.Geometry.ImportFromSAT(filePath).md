## Em profundidade
Geometry ImportFromSAT importa a geometria para o Dynamo de um tipo de arquivo SAT. Esse nó assume um filePath como entrada e também aceita uma sequência de caracteres com um caminho de arquivo válido. Para o exemplo abaixo, anteriormente exportamos a geometria para um arquivo SAT (consulte ExportToSAT). O nome do arquivo escolhido foi example.sat e foi exportado para uma pasta na área de trabalho dos usuários. No exemplo, mostramos dois nós diferentes usados para importar geometria de um arquivo SAT. Um deles tem um filePath como o tipo de entrada e o outro tem um `file` como o tipo de entrada. O filePath é criado usando um nó FilePath, que pode selecionar um arquivo clicando no botão Procurar. No segundo exemplo, especificamos o caminho do arquivo manualmente usando um elemento de sequência de caracteres.
___
## Arquivo de exemplo

![ImportFromSAT (filePath)](./Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(filePath)_img.jpg)

