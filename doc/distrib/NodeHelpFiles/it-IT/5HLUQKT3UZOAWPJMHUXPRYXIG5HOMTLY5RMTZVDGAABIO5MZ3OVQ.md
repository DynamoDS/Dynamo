<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## In profondità
`Surface.Thicken (surface, thickness, both_sides)` crea un solido eseguendo l'offset di una superficie in base all'input `thickness` e terminando le estremità per chiudere il solido. Questo nodo ha un input aggiuntivo per specificare se ispessire o meno su entrambi i lati. L'input `both_sides` utilizza un valore booleano: True per ispessire su entrambi i lati e False per ispessire su un lato. Tenere presente che il parametro `thickness` determina lo spessore totale del solido finale, pertanto se `both_sides` è impostato su True, verrà eseguito l'offset del risultato rispetto alla superficie originale della metà dello spessore di input su entrambi i lati.

Nell'esempio seguente, viene creata una superficie utilizzando `Surface.BySweep2Rails`. Viene quindi creato un solido utilizzando un dispositivo di scorrimento numerico per determinare l'input `thickness` di un nodo `Surface.Thicken`. Un pulsante di commutazione di valori booleani controlla se ispessire su entrambi i lati o solo su uno.

___
## File di esempio

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
