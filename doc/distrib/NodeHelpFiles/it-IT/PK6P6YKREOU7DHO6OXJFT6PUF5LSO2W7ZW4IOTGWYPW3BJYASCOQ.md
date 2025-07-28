<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` definisce se la geometria T-Spline presenta una simmetria radiale. È possibile introdurre la simmetria radiale solo per le primitive T-Spline che lo consentono: cono, sfera, rivoluzione, toro. Una volta stabilita alla creazione della geometria T-Spline, la simmetria radiale influenza tutte le successive operazioni e alterazioni.

Per applicare la simmetria, è necessario definire il numero desiderato di `symmetricFaces`, con 1 come valore minimo. Indipendentemente dal numero di campate iniziali di raggio e altezza della superficie T-Spline, questa verrà ulteriormente divisa nel numero scelto di `symmetricFaces`.

Nell'esempio seguente, viene creato `TSplineSurface.ByConePointsRadii` e viene applicata la simmetria radiale tramite l'uso del nodo `TSplineInitialSymmetry.ByRadial`. I nodi `TSplineTopology.RegularFaces` e `TSplineSurface.ExtrudeFaces` vengono quindi utilizzati rispettivamente per selezionare ed estrudere una faccia della superficie T-Spline. L'estrusione viene applicata simmetricamente e il dispositivo di scorrimento per il numero di facce simmetriche mostra come vengono suddivise le campate radiali.

## File di esempio

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
