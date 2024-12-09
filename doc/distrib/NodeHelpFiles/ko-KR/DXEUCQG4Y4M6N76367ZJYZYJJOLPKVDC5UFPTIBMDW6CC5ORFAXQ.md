<!--- Autodesk.DesignScript.Geometry.CoordinateSystem.Scale(coordinateSystem, basePoint, from, to) --->
<!--- DXEUCQG4Y4M6N76367ZJYZYJJOLPKVDC5UFPTIBMDW6CC5ORFAXQ --->
## 상세
`CoordinateSystem.Scale (coordinateSystem, basePoint, from, to, to)`은 축척 기준점, 축척 시작점 및 축척 대상점을 기준으로 축척된 CoordinateSystem을 반환합니다. `basePoint` 입력은 축척이 시작되는 위치(CoordinateSystem이 이동되는 정도)를 정의합니다. `from` 및 `to` 점 사이의 거리는 축척을 조정할 크기를 정의합니다.

아래 예에서 `basePoint` (-1, 2, 0)은 축척 시작 위치를 정의합니다. `from` (1, 1, 0)과 `to` (6, 6, 0) 점 사이의 거리에 따라 축척을 조정할 크기가 결정됩니다.

___
## 예제 파일

![CoordinateSystem.Scale](./DXEUCQG4Y4M6N76367ZJYZYJJOLPKVDC5UFPTIBMDW6CC5ORFAXQ_img.jpg)
