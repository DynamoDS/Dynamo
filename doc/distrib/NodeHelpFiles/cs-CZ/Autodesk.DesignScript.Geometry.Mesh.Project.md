## Podrobnosti
Uzel `Mesh.Project` vrací bod na vstupní síti, který je projekcí vstupního bodu na síti ve směru daného vektoru. Aby uzel fungoval správně, měla by se čára nakreslená ze vstupního bodu ve směru vstupního vektoru protínat se zadanou sítí.

Ukázkový graf znázorňuje jednoduchý případ použití toho, jak uzel funguje. Vstupní bod je nad kulovou sítí, ale ne přímo na jejím vrcholu. Bod se promítne ve směru záporného vektoru osy 'Z'. Výsledný bod se promítne na kouli a zobrazí se přímo pod vstupním bodem. To se liší od výstupu uzlu `Mesh.Nearest` (na vstupu se použije stejný bod a síť), kde výsledný bod leží na síti podél „normálového vektoru“ procházejícího vstupním bodem (nejbližším bodem). Uzel `Line.ByStartAndEndPoint` se použije k zobrazení „trajektorie“ promítnutého bodu na síť.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.Project_img.jpg)
