## In profondità
Il nodo `Curve Mapper` utilizza curve matematiche per ridistribuire i punti all'interno di un intervallo definito. La ridistribuzione in questo contesto implica la riassegnazione delle coordinate X alle nuove posizioni lungo una curva specificata in base alle relative coordinate Y. Questa tecnica è particolarmente utile per applicazioni quali la progettazione di facciate, strutture parametriche del tetto e altri calcoli di progettazione in cui sono richiesti distribuzioni o motivi specifici.

Definire i limiti per le coordinate X e Y impostando i valori minimo e massimo. Questi limiti impostano i contorni entro i quali i punti verranno ridistribuiti. Quindi, selezionare una curva matematica tra le opzioni fornite, che include curve lineari, seno, coseno, del rumore di Perlin, di Bézier, gaussiane, paraboliche, di radice quadrata e di potenza. Utilizzare i punti di controllo interattivi per regolare la forma della curva selezionata, adattandola alle proprie esigenze specifiche.

È possibile bloccare la forma della curva utilizzando il pulsante di blocco, che impedisce di apportare ulteriori modifiche. Inoltre, è possibile ripristinare lo stato di default della forma utilizzando il pulsante di ripristino all'interno del nodo.

Specificare il numero di punti da ridistribuire impostando l'input Count. Il nodo calcola nuove coordinate X per il numero specificato di punti in base alla curva selezionata e ai limiti definiti. I punti vengono ridistribuiti in modo che le relative coordinate X seguano la forma della curva lungo l'asse Y.

Ad esempio, per ridistribuire 80 punti lungo una curva seno, impostare Min X su 0, Max X su 20, Min Y su 0 e Max Y su 10. Dopo aver selezionato la curva seno e averne regolato la forma in base alle esigenze, il nodo `Curve Mapper` genera 80 punti con le coordinate X che seguono il modello della curva seno lungo l'asse Y da 0 a 10.




___
## File di esempio

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
