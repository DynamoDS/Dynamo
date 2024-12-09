## In profondità

XY Line Plot crea un grafico con una o più linee tracciate in base agli input x-values e y-values. Etichettare le linee o modificare il numero di linee immettendo un elenco di etichette stringa nell'input labels. Ogni etichetta crea una nuova linea con codifica a colori. Se si inserisce un solo valore stringa, verrà creata una sola linea.

Per determinare il posizionamento di ciascun punto lungo ogni linea, utilizzare un elenco di elenchi contenenti valori doppi per gli input x-values e y-values. È necessario che vi sia un numero uguale di valori negli input x-values e y-values. Il numero di sottoelenchi deve inoltre corrispondere al numero di valori stringa nell'input labels.
Ad esempio, se si desidera creare 3 linee, ciascuna con 5 punti, fornire un elenco con 3 valori stringa nell'input labels per denominare ciascuna linea e fornire 3 sottoelenchi con 5 valori doppi in ciascuno per gli input x-values e y-values.

Per assegnare un colore per ciascuna linea, inserire un elenco di colori nell'input colors. Quando si assegnano colori personalizzati, il numero di colori deve corrispondere al numero di valori stringa nell'input labels. Se non vengono assegnati colori, verranno utilizzati colori casuali.

___
## File di esempio

![XY Line Plot](./CoreNodeModelsWpf.Charts.XYLineChartNodeModel_img.jpg)

