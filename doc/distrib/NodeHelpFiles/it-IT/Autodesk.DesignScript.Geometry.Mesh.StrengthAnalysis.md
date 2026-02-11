## In profondità
 Il nodo `Mesh.StrengthAnalysis` restituisce un elenco di colori rappresentativi per ogni vertice. Il risultato può essere utilizzato insieme al nodo `Mesh.ByMeshColor`. Le aree più forti della mesh sono di colore verde, mentre quelle più deboli sono indicate da una mappa di calore da giallo a rosso. L'analisi può restituire falsi positivi se la mesh è troppo grossolana o irregolare (ossia contiene molti triangoli lunghi e sottili). Si potrebbe provare ad utilizzare `Mesh.Remesh` per generare una mesh normale prima di chiamare `Mesh.StrengthAnalysis` su di essa per generare risultati migliori.

Nell'esempio seguente, `Mesh.StrengthAnalysis` viene utilizzato per codificare a colori la resistenza strutturale di una mesh a forma di griglia. Il risultato è un elenco di colori corrispondenti alla lunghezza dei vertici della mesh. Questo elenco può essere utilizzato con il nodo `Mesh.ByMeshColor` per colorare la mesh.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
