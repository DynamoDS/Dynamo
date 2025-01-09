## In-Depth
`TSplineSurface.BuildPipes` genera una superficie di tubazioni T-Spline utilizzando una rete di curve. Le singole tubazioni vengono considerate unite se i rispettivi punti finali rientrano nella tolleranza massima impostata dall'input `snappingTolerance`. Il risultato di questo nodo può essere perfezionato con un gruppo di input che consentono di impostare i valori per tutte le tubazioni o singolarmente, se l'input è un elenco di lunghezza uguale al numero di tubazioni. I seguenti input possono essere utilizzati in questo modo: `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` ed `endPositions`.

Nell'esempio seguente, tre curve unite in corrispondenza dei punti finali vengono fornite come input per il nodo `TSplineSurface.BuildPipes`. `defaultRadius` in questo caso è un valore singolo per tutte e tre le tubazioni, definendo per default il raggio delle tubazioni, a meno che non vengano forniti raggi iniziali e finali.
Quindi, `segmentCount` imposta tre valori diversi per ogni singola tubazione. L'input è un elenco di tre valori, ciascuno corrispondente ad una tubazione.

Se `autoHandleStart` e `autoHandleEnd` sono impostati su False, sono disponibili ulteriori regolazioni. Ciò consente di controllare le rotazioni iniziali e finali di ogni tubazione (input`startRotations` ed `endRotations`), nonché i raggi alla fine e all'inizio di ogni tubazione, specificando `startRadii` ed `endRadii`. Infine, `startPositions` ed `endPositions` consentono l'offset dei segmenti rispettivamente all'inizio o alla fine di ogni curva. Questo input prevede un valore corrispondente al parametro della curva dove i segmenti iniziano o finiscono (valori compresi tra 0 e 1).

## File di esempio
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
