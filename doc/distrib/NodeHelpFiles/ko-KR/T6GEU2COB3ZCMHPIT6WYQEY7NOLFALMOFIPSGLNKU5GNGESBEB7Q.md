<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## 상세
`NurbsCurve.ByControlPointsWeightsKnots`를 사용하면 NurbsCurve의 가중치 및 노트를 수동으로 제어할 수 있습니다. 가중치 리스트는 제어점 리스트와 같은 길이여야 합니다. 노트 리스트의 크기는 제어점 수에 각도를 더한 값에 1을 더한 값과 같아야 합니다.

아래 예에서는 먼저 일련의 임의의 점 사이를 보간하여 NurbsCurve를 만듭니다. 노트, 가중치 및 제어점을 사용하여 해당 곡선의 해당 부분을 구합니다. `List.ReplaceItemAtIndex`를 사용하면 가중치 리스트를 수정할 수 있습니다. 마지막으로 `NurbsCurve.ByControlPointsWeightsKnots`를 사용하여 수정된 가중치로 NurbsCurve를 다시 만듭니다.

___
## 예제 파일

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

