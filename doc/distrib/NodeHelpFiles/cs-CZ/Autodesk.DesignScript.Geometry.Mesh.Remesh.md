## Podrobnosti
Uzel `Mesh.Remesh` vytvoří novou síť, ve které jsou trojúhelníky v daném objektu rovnoměrněji rozloženy bez ohledu na jakoukoli změnu normál trojúhelníků. Tato operace může být užitečná u sítí s proměnnou hustotou trojúhelníků za účelem přípravy sítě na pevnostní analýzu. Opakovaným přesíťováním sítě vznikají postupně jednotnější sítě. U sítí, jejichž vrcholy jsou již ve stejné vzdálenosti (například síť ikosféry), je výsledkem uzlu `Mesh.Remesh` stejná síť.
V následujícím příkladu se uzel `Mesh.Remesh` použije na importovanou síť s vysokou hustotou trojúhelníků v oblastech s jemnými detaily. Výsledek uzlu `Mesh.Remesh` se přesunut na stranu a k vizualizaci výsledku se použije uzel `Mesh.Edges`.

`(Vzorový soubor je licencován pod licencí Creative Commons)`

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.Remesh_img.jpg)
