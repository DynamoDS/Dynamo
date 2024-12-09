## Podrobnosti
Uzel `Mesh.Explode` vezme jednu síť a vrátí seznam ploch sítě jako nezávislých sítí.

Následující příklad ukazuje síťovou kopuli, která je rozložena pomocí uzlu `Mesh.Explode`, následovanou odsazením každé plochy ve směru normály plochy. Toho je dosaženo pomocí uzlů `Mesh.TriangleNormals` a `Mesh.Translate`. I když se v tomto příkladu plochy sítě jeví jako čtyřúhelníky, ve skutečnosti se jedná o trojúhelníky se stejnými normálami.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
