## In profondità
Repair tenterà di riparare superfici o PolySurface, che presentano una geometria non valida, nonché di eseguire potenzialmente ottimizzazioni. Il nodo Repair restituirà un nuovo oggetto superficie.
Questo nodo è utile quando si verificano errori durante l'esecuzione di operazioni su geometria importata o convertita.

Ad esempio, se si importano dati da un contesto host come **Revit** o da un file **.SAT** e si rileva che in maniera imprevista non riesce l'operazione booleana o di ritaglio, è possibile che un'operazione di correzione corregga qualsiasi *geometria non valida* che causa l'errore.

In generale, non è necessario utilizzare questa funzionalità sulla geometria creata in Dynamo, solo sulla geometria proveniente da origini esterne. Se non è così, segnalare un bug al team di Dynamo su github.
___


