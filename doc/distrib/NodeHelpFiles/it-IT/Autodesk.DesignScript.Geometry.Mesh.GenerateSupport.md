## In profondità
Il nodo `Mesh.GenerateSupport` viene utilizzato per aggiungere supporti alla geometria della mesh di input al fine di prepararla per la stampa 3D. I supporti sono necessari per stampare correttamente la geometria con sporgenze al fine di garantire la corretta aderenza dei layer ed evitare che il materiale si afflosci durante il processo di stampa. `Mesh.GenerateSupport` rileva le sporgenze e genera automaticamente supporti ad albero che consumano meno materiale e possono essere rimossi più facilmente, avendo meno contatto con la superficie stampata. Nei casi in cui non vengono rilevate sporgenze, il risultato del nodo `Mesh.GenerateSupport` è la stessa mesh, ruotata e in un orientamento ottimale per la stampa e traslata rispetto al piano XY. La configurazione dei supporti viene controllata dagli input:
- baseHeight definisce lo spessore della parte più bassa del supporto, è la sua base.
- baseDiameter controlla le dimensioni della base del supporto.
- postDiameter controlla la dimensione di ogni supporto nella sua metà.
- tipHeight e tipDiameter controllano le dimensioni dei supporti sulla loro punta, a contatto con la superficie stampata.
Nell'esempio seguente, il nodo `Mesh.GenerateSupport` viene utilizzato per aggiungere supporti ad una mesh a forma di lettera ‘T’.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)
