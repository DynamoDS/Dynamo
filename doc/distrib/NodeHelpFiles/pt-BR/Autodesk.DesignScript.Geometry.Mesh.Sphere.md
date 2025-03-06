## Em profundidade
'Mesh.Sphere' cria uma esfera em malha, centralizada no ponto de entrada 'origin', com um determinado 'raio' e um número de 'divisões'. A entrada booleana 'icosphere' é usada para alternar entre os tipos de malha esférica “icosfera” e “Esfera UV”. Uma malha de icosfera cobre a esfera com triângulos mais regulares do que uma malha de UV e tende a fornecer melhores resultados em operações de modelagem de nível superior. Uma malha UV tem seus polos alinhados com o eixo da esfera e as camadas de triângulo são geradas longitudinalmente em torno do eixo.

No caso da icosfera, o número de triângulos em torno do eixo da esfera poderia ser tão baixo quanto o número especificado de divisões e no máximo o dobro desse número. As divisões de uma “esfera UV” determinam o número de camadas de triângulo geradas longitudinalmente em torno da esfera. Quando a entrada 'divisions' é definida como zero, o nó retorna uma esfera UV com um número padrão de 32 divisões para cada tipo de malha.

No exemplo abaixo, o nó 'Mesh.Sphere' é usado para criar duas esferas com raios e divisões idênticos, mas usando métodos diferentes. Quando a entrada 'icosphere' é definida como 'True', 'Mesh.Sphere' retorna uma “icosfera”. Como alternativa, quando a entrada 'icosphere' é definida como 'False', o nó 'Mesh.Sphere' retorna uma “esfera UV”.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
