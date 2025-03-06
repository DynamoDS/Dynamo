## In profondità
GetColorAtParameter utilizza un intervallo di colori 2D di input e restituisce un elenco di colori in corrispondenza dei parametri UV specificati nell'intervallo compreso tra 0 e 1. Nell'esempio seguente, viene prima creato un intervallo di colori 2D utilizzando un nodo ByColorsAndParameters con un elenco di colori e un elenco di parametri per impostare l'intervallo. Viene utilizzato un Code Block per generare un intervallo di numeri compreso tra 0 e 1, che viene utilizzato come input u e v in un nodo UV.ByCoordinates. Il collegamento di questo nodo è impostato su Globale. Un insieme di cubi viene creato in modo simile, che un nodo Point.ByCoordinates con collegamento Globale ha utilizzato per creare una matrice di cubi. Viene quindi utilizzato un nodo Display.ByGeometryColor con la matrice di cubi e l'elenco di colori ottenuti dal nodo GetColorAtParameter.
___
## File di esempio

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

