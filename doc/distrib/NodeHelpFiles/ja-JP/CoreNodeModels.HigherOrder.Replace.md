## 詳細
ReplaceByCondition は、指定されたリストを取得し、各項目を指定された条件で評価します。条件で「true」と評価された場合は、出力されたリストでその対応する項目が replaceWith 入力で指定された項目で置換されます。次の例では、Formula ノードに式 x%2==0 を入力し、指定された項目を 2 で除算した後の余りを求めて 0 と等しいかどうかを確認します。偶数の整数の場合は、この式は「true」を返します。入力 x は空白になっています。ReplaceByCondition ノードの条件としてこの式を使用すると、出力されるリストでは、各偶数が指定された項目(このサンプルでは整数 10)で置換されています。
___
## サンプル ファイル

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

