## Em profundidade
'Mesh.Cone' cria um cone de malha cuja base é centralizada em um ponto de origem de entrada, com um valor de entrada para raios base e superior, altura e um número de “divisões”. O número de “divisões” corresponde ao número de vértices que são criados no topo e na base do cone. Se o número de “divisões” for 0, o Dynamo usará um valor padrão. O número de divisões ao longo do eixo Z é sempre igual a 5. A entrada 'cap' usa um “booleano” para controlar se o cone está fechado na parte superior.
No exemplo abaixo, o nó 'Mesh.Cone' é usado para criar uma malha em forma de cone com 6 divisões, portanto, a base e o topo do cone são hexágonos. O nó 'Mesh.Triangles' é usado para visualizar a distribuição de triângulos de malha.


## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
