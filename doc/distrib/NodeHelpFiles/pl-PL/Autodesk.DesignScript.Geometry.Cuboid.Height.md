## Informacje szczegółowe
Węzeł `Cuboid.Height` zwraca wysokość prostopadłościanu wejściowego. Uwaga: jeśli prostopadłościan przekształcono do innego układu współrzędnych za pomocą współczynnika skali, ten węzeł zwróci pierwotne wymiary prostopadłościanu, a nie wymiary przestrzeni globalnej. Innymi słowy: jeśli utworzysz prostopadłościan o szerokości (oś X) równej 10 i przekształcisz go do układu CoordinateSystem ze skalowaniem 2-krotnym na osi X, szerokość nadal będzie równa 10.

W poniższym przykładzie generujemy prostopadłościan na podstawie narożników, a następnie za pomocą węzła `Cuboid.Height` znajdujemy jego wysokość.

___
## Plik przykładowy

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

