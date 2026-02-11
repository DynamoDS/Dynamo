## In profondità

Il nodo Define Data convalida il tipo di dati in entrata. Può essere utilizzato per garantire che i dati locali siano del tipo desiderato ed è anche progettato per essere usato come nodo di input o di output che dichiara il tipo di dati previsti o forniti da un grafico. Il nodo supporta una selezione di tipi di dati di Dynamo comunemente utilizzati, ad esempio "String", "Point" o "Boolean". L'elenco completo dei tipi di dati supportati è disponibile nel menu a discesa del nodo. Il nodo supporta dati sotto forma di un singolo valore o di un elenco non strutturato. Non sono supportati elenchi nidificati, dizionari e repliche.

### Comportamento

Il nodo convalida i dati provenienti dalla porta di input in base all'impostazione del menu a discesa Tipo e del pulsante di commutazione Elenco (vedere di seguito per i dettagli). Se la convalida è stata eseguita correttamente, l'output del nodo corrisponde all'input. Se la convalida non è stata eseguita correttamente, il nodo entrerà in uno stato di avviso con un output nullo.
Il nodo presenta un input:

-   L'input "**>**": si connette ad un nodo a monte per convalidarne il tipo di dati.
    Inoltre, il nodo offre tre controlli utente:
-   Il pulsante di commutazione **Rileva automaticamente tipo**: quando è attivato, il nodo analizza i dati in entrata e, se sono di un tipo supportato, imposta i valori dei controlli Tipo ed Elenco in base al tipo di dati in entrata. Il menu a discesa Tipo e il pulsante di commutazione Elenco sono disattivati e verranno aggiornati automaticamente in base al nodo di input.

    Quando l'opzione Rileva automaticamente tipo è disattivata, è possibile specificare un tipo di dati utilizzando il menu Tipo e il pulsante di commutazione Elenco. Se i dati in entrata non corrispondono ai criteri specificati, il nodo entrerà in uno stato di avviso con un output nullo.
-   Il menu a discesa **Tipo**: imposta il tipo di dati previsto. Quando il controllo è attivato (il pulsante di commutazione **Rileva automaticamente tipo** è disattivato), impostare un tipo di dati per la convalida. Quando il controllo è disattivato (il pulsante di commutazione **Rileva automaticamente tipo** è attivato), il tipo di dati viene impostato automaticamente in base ai dati in entrata. I dati sono validi se il loro tipo corrisponde esattamente a quello mostrato o se il loro tipo è un elemento derivato di quello mostrato (ad esempio, se il menu a discesa Tipo è impostato su "Curva", sono validi gli oggetti di tipo "Rettangolo", "Linea", ecc.).
-   Il pulsante di commutazione **Elenco**: quando è attivato, il nodo prevede che i dati in entrata siano un singolo elenco non strutturato contenente elementi di un tipo di dati valido (vedere sopra). Quando è disattivato, il nodo prevede un singolo elemento di un tipo di dati valido.

### Usa come nodo di input

Quando è impostato come input ("È input" nel menu contestuale del nodo), il nodo può facoltativamente utilizzare i nodi a monte per impostare il valore di default dell'input. Un'esecuzione del grafico memorizzerà nella cache il valore del nodo Define Data per utilizzarlo quando si esegue il grafico esternamente, ad esempio con il nodo Engine.

## File di esempio

Nell'esempio seguente, il primo gruppo di nodi "DefineData" presenta il pulsante di commutazione **Rileva automaticamente tipo** disattivato. Il nodo convalida correttamente l'input Number fornito, mentre rifiuta l'input String. Il secondo gruppo contiene un nodo con il pulsante di commutazione **Rileva automaticamente tipo** attivato. Il nodo regola automaticamente l'elenco a discesa Tipo e il pulsante di commutazione Elenco in base all'input, in questo caso un elenco di numeri interi.

![Define_Data](./CoreNodeModels.DefineData_img.png)
