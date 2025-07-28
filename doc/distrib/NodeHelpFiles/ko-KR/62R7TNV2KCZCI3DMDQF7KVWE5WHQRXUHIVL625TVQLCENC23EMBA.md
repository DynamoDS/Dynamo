<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## 상세
`Surface.ToNurbsSurface`는 표면을 입력으로 사용하고 입력 표면과 근사한 NurbsSurface를 반환합니다. `limitSurface` 입력은 표면의 매개변수 범위가 자르기 작업 이후에 제한되는 경우처럼 변환하기 전에 표면을 원래 매개변수 범위로 복원할지 여부를 결정합니다.

아래 예에서는 닫힌 NurbsCurve를 입력으로 하여 `Surface.ByPatch` 노드를 사용해 표면을 만듭니다. 이 표면을 `Surface.ToNurbsSurface` 노드의 입력으로 사용하면 4개의 면이 있는 잘리지 않은 NurbsSurface가 생성됩니다.


___
## 예제 파일

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
