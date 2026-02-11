<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## 상세
`TSplineSurface.CompressIndexes` 노드는 면 삭제와 같은 다양한 작업으로 인해 발생하는 T-Spline 표면의 모서리, 정점 또는 면 색인 번호의 간격을 제거합니다. 색인 순서는 유지됩니다.

아래 예에서는 모양의 모서리 색인에 영향을 주는 쿼드볼 원형 표면에서 여러 면이 삭제됩니다. `TSplineSurface.CompressIndexes`는 모양의 모서리 색인을 복구하는 데 사용되므로 색인이 1인 모서리를 선택하는 것이 가능해집니다.

## 예제 파일

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
