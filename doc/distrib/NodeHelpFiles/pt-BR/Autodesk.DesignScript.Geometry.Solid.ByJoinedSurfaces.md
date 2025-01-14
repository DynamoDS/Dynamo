## Em profundidade
Solid by Joined Surfaces usa uma lista de superfícies como entrada e retorna um único sólido definido pelas superfícies. As superfícies devem definir uma superfície fechada. No exemplo abaixo, começamos com um círculo como uma geometria base. O círculo é corrigido para criar uma superfície, e essa superfície é convertida na direção z. Em seguida, extraímos o círculo para produzir os lados. List.Create é usado para criar uma lista que consiste na base, lado e superfícies superiores e, em seguida, usamos ByJoinedSurfaces para transformar a lista em um único sólido fechado.
___
## Arquivo de exemplo

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

