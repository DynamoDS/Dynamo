## In-Depth

Nell'esempio seguente, viene eseguita l'operazione di annullamento della saldatura su una riga di bordi di una superficie T-Spline. Di conseguenza, i vertici dei bordi selezionati vengono divisi. A differenza dell'annullamento della triangolazione, che crea una transizione netta attorno al bordo mantenendo la connessione, l'annullamento della saldatura crea una discontinuità. Ciò può essere dimostrato confrontando il numero di vertici prima e dopo l'operazione. Qualsiasi operazione successiva sui bordi o sui vertici non saldati mostrerà inoltre che la superficie viene disconnessa lungo il bordo non saldato.

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
