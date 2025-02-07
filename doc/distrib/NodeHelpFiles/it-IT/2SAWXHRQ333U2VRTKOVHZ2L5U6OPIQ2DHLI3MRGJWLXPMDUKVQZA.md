<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## In profondità
Crea una NurbsSurface con vertici di controllo, nodi, pesi e gradi U e V specificati. Sono presenti varie restrizioni dei dati che, in caso di violazione, restituiscono un errore e generano un'eccezione. Grado: i gradi U- e V- devono essere >= 1 (spline lineare a tratti) e inferiori a 26 (il grado massimo di base di B-spline supportato da ASM). Pesi: tutti i valori di peso (se forniti) dovranno essere positivi. I pesi inferiori a 1e-11 verranno respinti e la funzione produrrà un errore. Nodi: entrambi i vettori nodo dovranno essere sequenze non decrescenti. La molteplicità dei nodi interna non deve essere superiore a grado più 1 in corrispondenza del nodo iniziale/finale e a grado in corrispondenza di un nodo interno (ciò consente la rappresentazione di superfici con discontinuità G1). Si osservi che i vettori nodo non bloccati sono supportati ma verranno convertiti in bloccati e le modifiche corrispondenti verranno applicate ai dati del punto di controllo/peso.
___
## File di esempio



