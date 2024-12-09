## Podrobnosti
Uzel `TSplineSurface.FlattenVertices(vertices, parallelPlane)` mění pozice řídicích bodů pro zadanou sadu vrcholů zarovnáním s hodnotami zadanými na vstupu `parallelPlane`.

V níže uvedeném příkladu jsou vrcholy povrchu roviny T-Spline posunuty pomocí uzlů `TsplineTopology.VertexByIndex` a `TSplineSurface.MoveVertices`. Povrch je poté přesunut na stranu k lepšímu náhledu a použije se jako vstup pro uzel `TSplineSurface.FlattenVertices(vertices, parallelPlane)`. Výsledkem je nový povrch s vybranými vrcholy ležícími na dané rovině.
___
## Vzorový soubor

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
