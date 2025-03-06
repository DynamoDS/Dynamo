## In profondità

Bar Chart crea un grafico con barre orientate verticalmente. Le barre possono essere organizzate in più gruppi ed etichettate con una codifica a colori. È possibile creare un singolo gruppo immettendo un unico valore doppio o più gruppi immettendo più valori doppi per ogni sottoelenco nell'input values. Per definire le categorie, inserire un elenco di valori stringa nell'input labels. Ogni valore crea una nuova categoria con codifica a colori.

Per assegnare un valore (altezza) a ciascuna barra, immettere un elenco di elenchi contenenti valori doppi nell'input values. Ogni sottoelenco determinerà il numero di barre e la categoria a cui appartengono, nello stesso ordine dell'input labels. Se si dispone di un singolo elenco, verrà creata solo una categoria. Il numero di valori stringa nell'input labels deve corrispondere al numero di sottoelenchi nell'input values.

Per assegnare un colore a ciascuna categoria, inserire un elenco di colori nell'input colors. Quando si assegnano colori personalizzati, il numero di colori deve corrispondere al numero di valori stringa nell'input labels. Se non vengono assegnati colori, verranno utilizzati colori casuali.

## Esempio: singolo gruppo

Si supponga di voler rappresentare le classificazioni medie degli utenti per un articolo nei primi tre mesi dell'anno. Per visualizzare questa visualizzazione, è necessario un elenco di tre valori stringa, etichettati January, February e March.
Così, per l'input labels, in un Code Block verrà fornito il seguente elenco:

[“January”, “February”, “March”];

È inoltre possibile utilizzare i nodi String connessi al nodo List Create per creare l'elenco.

Successivamente, nell'input values, verrà immessa la classificazione media degli utenti per ciascuno dei tre mesi come elenco di elenchi:

[[3.5], [5], [4]];

Notare che, poiché sono disponibili tre etichette, sono necessari tre sottoelenchi.

Ora, quando il grafico viene eseguito, viene creato il grafico a barre, con ogni barra colorata che rappresenta la valutazione media dei clienti per il mese. È possibile continuare ad utilizzare i colori di default o inserire un elenco di colori personalizzati nell'input colors.

## Esempio: più gruppi

È possibile sfruttare la funzionalità di raggruppamento del nodo Bar Chart immettendo più valori in ogni sottoelenco nell'input values. In questo esempio, verrà creato un grafico che visualizza il numero di porte in tre variazioni di tre modelli, Model A, Model B e Model C.

A tale scopo, verranno fornite innanzitutto le etichette:

[“Model A”, “Model B”, “Model C”];

Successivamente, verranno forniti i valori, assicurandosi di nuovo che il numero di sottoelenchi corrisponda al numero di etichette:

[[17, 9, 13],[12,11,15],[15,8,17]];

Ora, quando si fa clic su Esegui, il nodo Bar Chart creerà un grafico con tre gruppi di barre, contrassegnati rispettivamente come Index 0, 1 e 2, In questo esempio, considerare ogni indice (ovvero un gruppo) una variante progettuale. I valori del primo gruppo (Index 0) vengono estratti dalla prima voce di ogni elenco nell'input values, pertanto il primo gruppo contiene 17 per Model A, 12 per Model B e 15 per Model C. Il secondo gruppo (Index 1) utilizza il secondo valore in ogni gruppo e così via.

___
## File di esempio

![Bar Chart](./CoreNodeModelsWpf.Charts.BarChartNodeModel_img.jpg)

