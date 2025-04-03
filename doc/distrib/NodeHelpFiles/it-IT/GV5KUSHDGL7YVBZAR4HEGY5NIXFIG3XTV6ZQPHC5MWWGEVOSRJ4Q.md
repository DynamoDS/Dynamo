## In profondità
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Definire i limiti per le coordinate X e Y impostando i valori minimo e massimo. Questi limiti impostano i contorni entro i quali i punti verranno ridistribuiti. Quindi, selezionare una curva matematica tra le opzioni fornite, che include curve lineari, seno, coseno, del rumore di Perlin, di Bézier, gaussiane, paraboliche, di radice quadrata e di potenza. Utilizzare i punti di controllo interattivi per regolare la forma della curva selezionata, adattandola alle proprie esigenze specifiche.

È possibile bloccare la forma della curva utilizzando il pulsante di blocco, che impedisce di apportare ulteriori modifiche. Inoltre, è possibile ripristinare lo stato di default della forma utilizzando il pulsante di ripristino all'interno del nodo.

Specificare il numero di punti da ridistribuire impostando l'input Count. Il nodo calcola nuove coordinate X per il numero specificato di punti in base alla curva selezionata e ai limiti definiti. I punti vengono ridistribuiti in modo che le relative coordinate X seguano la forma della curva lungo l'asse Y.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## File di esempio


