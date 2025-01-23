## In profondità

Heat Series Plot crea un grafico in cui i punti dati vengono rappresentati come rettangoli in colori diversi lungo un intervallo di colori.

Assegnare etichette per ogni colonna e riga immettendo un elenco di etichette stringa rispettivamente negli input x-labels e y-labels. Il numero di input x-labels e quello di input y-labels non devono corrispondere.

Definire un valore per ogni rettangolo con l'input values. Il numero di sottoelenchi deve corrispondere al numero di valori stringa nell'input x-labels, in quanto rappresenta il numero di colonne. I valori all'interno di ogni sottoelenco rappresentano il numero di rettangoli in ogni colonna. Ad esempio, 4 sottoelenchi corrispondono a 4 colonne e, se ogni sottoelenco ha 5 valori, le colonne hanno 5 rettangoli ciascuna.

Come ulteriore esempio, per creare una griglia con 5 righe e 5 colonne, fornire 5 valori stringa nell'input x-labels e nell'input y-labels. I valori x-labels compariranno sotto il grafico lungo l'asse x e i valori y-labels compariranno a sinistra del grafico lungo l'asse y.

Nell'input values, immettere un elenco di elenchi, con ogni sottoelenco contenente 5 valori. I valori vengono tracciati colonna per colonna da sinistra a destra e dal basso verso l'alto, pertanto il primo valore del primo sottoelenco è il rettangolo inferiore nella colonna a sinistra, il secondo valore è il rettangolo sopra di esso e così via. Ogni sottoelenco rappresenta una colonna nel grafico.

È possibile assegnare un intervallo di colori per differenziare i punti dati immettendo un elenco di valori del colore nell'input colors. Il valore più basso nel grafico sarà uguale al primo colore e il valore più alto sarà uguale all'ultimo colore, con altri valori intermedi per tutta la sfumatura. Se non viene assegnato alcun intervallo di colori, ai punti dati verrà assegnato un colore casuale dalla tonalità più chiara a quella più scura.

Per ottenere risultati ottimali, utilizzare uno o due colori. Il file di esempio fornisce un caso classico di due colori, blu e rosso. Quando vengono utilizzati come input colors, Heat Series Plot creerà automaticamente una sfumatura tra questi colori, con valori bassi rappresentati in tonalità di blu e valori alti in tonalità di rosso.

___
## File di esempio

![Heat Series Plot](./CoreNodeModelsWpf.Charts.HeatSeriesNodeModel_img.jpg)

