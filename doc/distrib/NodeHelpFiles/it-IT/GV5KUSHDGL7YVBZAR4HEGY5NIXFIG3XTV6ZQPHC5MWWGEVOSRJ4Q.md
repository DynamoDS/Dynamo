## In profondità
Il nodo `Curve Mapper` ridistribuisce una serie di valori di input all'interno di un intervallo definito e sfrutta le curve matematiche per mapparli lungo una curva specifica. In questo contesto, la mappatura significa che i valori vengono ridistribuiti in modo tale che le loro coordinate X seguano la forma della curva lungo l'asse Y. Questa tecnica è particolarmente preziosa per applicazioni come la progettazione di facciate, le strutture parametriche dei tetti e altri calcoli di progettazione che richiedono modelli o distribuzioni specifiche.

Definire i limiti per le coordinate X e impostando i valori minimo e massimo. Questi limiti impostano i contorni entro i quali i punti verranno ridistribuiti. È possibile fornire un singolo conteggio per generare una serie di valori uniformemente distribuiti oppure una serie di valori esistenti, che verranno distribuiti lungo la direzione X entro l'intervallo specificato e quindi mappati alla curva.

Selezionare una curva matematica tra le opzioni disponibili, che comprendono le curve lineare, seno, coseno, del rumore di Perlin, Bézier, gaussiana, parabolica, di radice quadrata e di potenza. Utilizzare i punti di controllo interattivi per regolare la forma della curva selezionata, adattandola alle esigenze specifiche.

È possibile bloccare la forma della curva utilizzando il pulsante di blocco, impedendo ulteriori modifiche alla curva. Inoltre, è possibile ripristinare lo stato di default della forma utilizzando il pulsante di ripristino all'interno del nodo. Se si ottengono NaN o Null come output, è possibile trovare ulteriori dettagli [qui] (https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues) sul possibile motivo di questo problema.

Ad esempio, per ridistribuire 80 punti lungo una curva seno entro un intervallo compreso tra 0 e 20, impostare Min su 0, Max su 20 e Values su 80. Dopo aver selezionato la curva seno e averne regolato la forma in base alle esigenze, il nodo `Curve Mapper` genera 80 punti con le coordinate X che seguono il modello della curva seno lungo l'asse Y.

Per mappare valori distribuiti in modo non uniforme lungo una curva gaussiana, impostare l'intervallo minimo e massimo e fornire la serie di valori. Dopo aver selezionato la curva gaussiana e averne regolato la forma in base alle esigenze, il nodo `Curve Mapper` ridistribuisce la serie di valori lungo le coordinate X utilizzando l'intervallo specificato e mappa i valori lungo il modello della curva. Per la documentazione approfondite sul funzionamento del nodo e su come impostare gli input, consultare [questo post del blog](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) incentrato su Curve Mapper.




___
## File di esempio

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
