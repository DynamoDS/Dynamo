## In profondità
Il nodo `Mesh.ExtrudePolyCurve` estrude una determinata `polycurve` in base ad una determinata distanza impostata dall'input `height` e nella direzione del vettore specificata. Le PolyCurve aperte vengono chiuse collegando il primo punto all'ultimo. Se la `polycurve` iniziale è piana e non autointersecante, la mesh risultante può essere chiusa per formare una mesh solida.
Nell'esempio seguente, `Mesh.ExtrudePolyCurve` viene utilizzato per creare una mesh chiusa basata su una PolyCurve chiusa.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.ExtrudePolyCurve_img.jpg)
