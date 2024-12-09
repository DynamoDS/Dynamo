## In profondità
`GeometryColor.ByMeshColor` restituisce un oggetto GeometryColor che è una mesh colorata che segue l'elenco di colori specificato. Esistono due modi per utilizzare questo nodo:

- Se viene fornito un colore, l'intera mesh viene colorata con un determinato colore.
- Se il numero di colori corrisponde al numero di triangoli, ogni triangolo viene colorato del colore corrispondente dell'elenco.
- Se il numero di colori corrisponde al numero di vertici univoci, il colore di ogni triangolo nel colore della mesh si interpola tra i valori del colore in corrispondenza di ogni vertice.
- Se il numero di colori è uguale al numero di vertici non univoci, il colore di ogni triangolo si interpola tra i valori del colore su una faccia, ma potrebbe non unirsi tra le facce.

## Esempio

Nell'esempio seguente, una mesh è codificata a colori in base alla quota altimetrica dei suoi vertici. Innanzitutto, `Mesh.Vertices` viene utilizzato per ottenere vertici della mesh univoci che vengono quindi analizzati e la quota altimetrica di ciascun punto del vertice viene ottenuta utilizzando il nodo `Point.Z`. In secondo luogo, `Map.RemapRange` viene utilizzato per associare i valori ad un nuovo intervallo compreso tra 0 e 1 ridimensionando ogni valore in modo proporzionale. Infine, `Color Range` viene utilizzato per generare un elenco di colori corrispondenti ai valori associati. Utilizzare questo elenco di colori come input `colors` del nodo `GeometryColor.ByMeshColors`. Il risultato è una mesh con codifica a colori in cui il colore di ciascun triangolo si interpola tra i colori dei vertici, dando luogo ad una sfumatura.

## File di esempio

![Example](./Modifiers.GeometryColor.ByMeshColors_img.jpg)
