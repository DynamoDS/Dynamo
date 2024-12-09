<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormal` genera una superficie del piano della primitiva T-Spline utilizzando un punto di origine e un vettore normale. Per creare il piano T-Spline, il nodo utilizza i seguenti input:
- `origin`: un punto che definisce l'origine del piano.
- `normal`: un vettore che specifica la direzione normale del piano creato.
- `minCorner` e `maxCorner`:gli angoli del piano, rappresentati come punti con valori X e Y (le coordinate Z verranno ignorate). Questi angoli rappresentano le estensioni della superficie T-Spline di output se viene convertita sul piano XY. I punti `minCorner` e `maxCorner` non devono coincidere con i vertici degli angoli in 3D. Ad esempio, quando `minCorner` è impostato su (0,0) e `maxCorner` è (5,10), la larghezza e la lunghezza del piano saranno rispettivamente 5 e 10.
- `xSpans` e `ySpans`: numero di campate/divisioni di larghezza e di lunghezza del piano
- `symmetry`: indica se la geometria è simmetrica rispetto ai suoi assi X, Y e Z
- `inSmoothMode`: indica se la geometria risultante verrà visualizzata con la modalità uniforme o riquadro

Nell'esempio seguente, viene creata una superficie T-Spline piana utilizzando il punto di origine fornito e la normale che è un vettore dell'asse X. La dimensione della superficie è controllata dai due punti utilizzati come input `minCorner` e `maxCorner`.

## File di esempio

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
