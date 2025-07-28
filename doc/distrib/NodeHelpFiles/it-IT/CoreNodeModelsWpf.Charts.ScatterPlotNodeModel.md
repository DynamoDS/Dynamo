## In profondità

Scatter Plot crea un grafico con punti tracciati in base agli input x-values e y-values e con codifica a colori in base al gruppo.
Etichettare i gruppi o modificare il numero di gruppi inserendo un elenco di valori stringa nell'input labels. Ogni etichetta crea un gruppo con codifica a colori corrispondente. Se si inserisce un solo valore stringa, tutti i punti saranno dello stesso colore e avranno un'etichetta condivisa.

Per determinare il posizionamento di ogni punto, utilizzare un elenco di elenchi contenenti valori doppi per gli input x-values e y-values. È necessario che vi sia un numero uguale di valori negli input x-values e y-values. Il numero di sottoelenchi deve inoltre essere allineato al numero di valori stringa nell'input labels.

Per assegnare un colore per ciascun gruppo, inserire un elenco di colori nell'input colors. Quando si assegnano colori personalizzati, il numero di colori deve corrispondere al numero di valori stringa nell'input labels. Se non viene assegnato alcun colore, verranno utilizzati i colori casuali.

___
## File di esempio

![Scatter Plot](./CoreNodeModelsWpf.Charts.ScatterPlotNodeModel_img.jpg)

