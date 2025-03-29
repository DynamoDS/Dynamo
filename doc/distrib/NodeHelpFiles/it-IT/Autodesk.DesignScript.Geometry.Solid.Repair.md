## In profondità
`Solid.Repair` tenterà di riparare solidi con geometria non valida, nonché di eseguire potenzialmente ottimizzazioni. `Solid.Repair` restituirà un nuovo oggetto solido.

Questo nodo è utile quando si verificano errori durante l'esecuzione di operazioni su geometria importata o convertita.

Nell'esempio seguente,`Solid.Repair` viene utilizzato per riparare la geometria da un file **.SAT**. L'operazione booleana o di taglio della geometria nel file non riesce e `Solid.Repair` corregge qualsiasi *geometria non valida* che causa l'errore.

In generale, non dovrebbe essere necessario utilizzare questa funzionalità sulla geometria creata in Dynamo, ma solo sulla geometria proveniente da origini esterne. In caso contrario, segnalare un bug al team di Dynamo su Github.
___
## File di esempio

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
