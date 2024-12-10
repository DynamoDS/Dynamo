## 상세
`PolySurface.BySweep (rail, crossSection)`은 레일을 따라 교차하지 않는 연결된 선 리스트를 스윕하여 PolySurface를 반환합니다. `crossSection` 입력은 시작점 또는 끝점에서 만나야 하는 연결된 곡선 리스트를 받을 수 있습니다. 그렇지 않으면 노드가 PolySurface를 반환하지 않습니다. 이 노드는 `PolySurface.BySweep (rail, profile)`과 유사합니다. 유일한 차이점은 `crossSection` 입력이 곡선 리스트를 가져오는 반면 `profile` 입력은 하나의 곡선만 사용한다는 점입니다.

아래 예에서는 호를 따라 스윕하여 PolySurface를 만듭니다.


___
## 예제 파일

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
