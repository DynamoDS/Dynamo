## Podrobnosti
Uzel `List.RemoveIfNot` vrací seznam, který obsahuje položky odpovídající danému typu prvku a odebere všechny ostatní položky v původním seznamu.

K odebrání položek možná bude nutné ve vstupu `type` použít úplnou cestu uzlu, například `Autodesk.DesignScript.Geometry.Surface`. Chcete-li načíst cesty položek seznamu, můžete seznam vložit do uzlu `Object.Type`.

V následujícím příkladu uzel `List.RemoveIfNot` vrátí seznam s jedním řádkem, přičemž odebere prvky bodů z původního seznamu, protože neodpovídají určenému typu.
___
## Vzorový soubor

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
