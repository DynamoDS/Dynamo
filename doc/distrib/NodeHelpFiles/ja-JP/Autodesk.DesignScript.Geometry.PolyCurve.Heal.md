## 詳細
`PolyCurve.Heal` は、自己交差する PolyCurve を取得し、自己交差しない新しい PolyCurve を返します。入力された PolyCurve は、3 つを上回る自己交差があってはなりません。つまり、PolyCurve の単一セグメントが他の 2 つを上回るセグメントと交差する場合、修復できません。`trimLength` に 0 より大きい値を入力すると、`trimLength` より長いセグメントはトリミングされません。

次の例では、`PolyCurve.Heal` を使用して自己交差する PolyCurve を修復します。
___
## サンプル ファイル

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
