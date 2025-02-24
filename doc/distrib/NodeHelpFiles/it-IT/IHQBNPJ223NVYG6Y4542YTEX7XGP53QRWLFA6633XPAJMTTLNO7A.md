<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## In profondità
`Surface.TrimWithEdgeLoops` taglia la superficie con una raccolta di una o più PolyCurve chiuse che devono trovarsi tutte sulla superficie entro la tolleranza specificata. Se uno o più fori devono essere tagliati dalla superficie di input, deve essere specificata una linea chiusa esterna per il contorno della superficie e una linea chiusa interna per ogni foro. Se l'area tra il contorno della superficie e i fori deve essere tagliata, deve essere fornita solo la linea chiusa per ogni foro. Per una superficie periodica senza linea chiusa esterna, ad esempio una superficie sferica, l'area tagliata può essere controllata invertendo la direzione della curva della linea chiusa.

La tolleranza è quella utilizzata quando si decide se le estremità della curva sono coincidenti e se una curva e una superficie sono coincidenti. La tolleranza fornita non può essere inferiore ad alcuna della tolleranze utilizzate nella creazione delle PolyCurve di input. Il valore di default 0.0 indica che verrà utilizzata la tolleranza più grande impiegata nella creazione delle PolyCurve di input.

Nell'esempio seguente, vengono tagliate da una superficie due linee chiuse, restituendo due nuove superfici evidenziate in blu. Il dispositivo di scorrimento numerico regola la forma delle nuove superfici.

___
## File di esempio

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
