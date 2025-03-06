## In profondità
L'operazione `Mesh.MakeHollow` può essere utilizzata per svuotare un oggetto mesh in preparazione per la stampa 3D. Lo svuotamento di una mesh può ridurre significativamente la quantità di materiale di stampa richiesto, i tempi e i costi di stampa. L'input `wallThickness` definisce lo spessore delle pareti dell'oggetto mesh. `Mesh.MakeHollow` può anche generare fori di fuga per rimuovere il materiale in eccesso durante il processo di stampa. Le dimensioni e il numero di fori vengono controllati dagli input `holeCount` e `holeRadius`. Infine, gli input `meshResolution` e `solidResolution` influiscono sulla risoluzione del risultato della mesh. Un valore `meshResolution` più alto migliora la precisione con cui la parte interna della mesh esegue l'offset della mesh originale, ma determinerà un maggior numero di triangoli. Un valore `solidResolution` più elevato migliora la misura in cui i dettagli più precisi della mesh originale vengono mantenuti nella parte interna della mesh cava.
Nell'esempio seguente, `Mesh.MakeHollow` viene utilizzato su una mesh a forma di cono. Alla base vengono aggiunti cinque fori di fuga.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
