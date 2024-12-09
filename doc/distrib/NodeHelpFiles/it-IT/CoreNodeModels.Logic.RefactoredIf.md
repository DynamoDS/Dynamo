## In profondità
RefactoredIf funge da un nodo di controllo condizionale. L'input "test" utilizza un valore booleano, mentre gli input "true" e "false" possono accettare qualsiasi tipo di dati. Se il valore di test è "true", il nodo restituirà l'elemento dall'input "true", se il test è "false", il nodo restituirà l'elemento dall'input "false". Nell'esempio seguente, viene prima generato un elenco di numeri casuali compresi tra 0 e 999. Il numero di elementi nell'elenco viene controllato da un Integer Slider. Si utilizza un Code Block con la formula "x%a==0" per verificare la divisibilità per un secondo numero, determinato da un secondo Number Slider. In questo modo viene generato un elenco di valori booleani corrispondenti al fatto che gli elementi nell'elenco casuale siano o meno divisibili per il numero determinato dal secondo Number Slider. Questo elenco di valori booleani viene utilizzato come input "test" per un nodo If. Si utilizza una sfera di default come input "true" e un cuboide di default come input "false". Il risultato del nodo If è un elenco di sfere o cuboidi. Infine, si utilizza un nodo Translate per separare l'elenco di geometrie.

RefactoredIf esegue la replica su tutti i nodi COME SE FOSSE IMPOSTATO SU SHORTEST. È possibile vedere il motivo di questa replica negli esempi allegati, soprattutto quando si osservano i risultati quando si applica LONGEST ad un nodo Formula e il ramo "short" della condizione viene fatto passare attraverso. Queste modifiche sono state inoltre apportate per consentire un comportamento prevedibile quando si utilizzano input booleani singoli o un elenco di valori booleani.
___
## File di esempio

![If](./CoreNodeModels.Logic.RefactoredIf_img.jpg)

