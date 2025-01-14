## In profondità
La modalità riquadro e la modalità uniforme sono due metodi per visualizzare una superficie T-Spline. La modalità uniforme è la forma reale di una superficie T-Spline ed è utile per visualizzare in anteprima le caratteristiche estetiche e dimensionali del modello. La modalità riquadro, invece, permette di vedere la struttura della superficie e di comprenderla meglio, oltre ad essere un'opzione più rapida per visualizzare in anteprima la geometria grande o complessa. La modalità riquadro e quella uniforme possono essere controllate al momento della creazione della superficie T-Spline iniziale o successivamente, con nodi come `TSplineSurface.EnableSmoothMode`.

Nei casi in cui una T-Spline non è più valida, la relativa anteprima passa automaticamente alla modalità riquadro. Il nodo `TSplineSurface.IsInBoxMode` è un altro modo per identificare se la superficie diventa non valida.

Nell'esempio seguente, viene creata una superficie del piano T-Spline con l'input `smoothMode` impostato su true. Due delle sue facce vengono eliminate, rendendo la superficie non valida. L'anteprima della superficie passa alla modalità riquadro, anche se è impossibile capirlo dalla sola anteprima. Il nodo `TSplineSurface.IsInBoxMode` viene utilizzato per confermare che la superficie è in modalità riquadro.
___
## File di esempio

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
