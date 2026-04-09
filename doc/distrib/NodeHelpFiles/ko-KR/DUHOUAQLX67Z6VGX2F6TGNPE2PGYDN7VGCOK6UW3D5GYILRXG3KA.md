<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## 상세
'Curve.SweepAsSurface'는 지정된 경로를 따라 입력 곡선을 스윕하여 표면을 작성합니다. 아래 예제에서는 코드 블록을 사용하여 스윕할 곡선을 작성하고 'Arc.ByThreePoints' 노드의 세 점을 작성합니다. 경로 곡선은 X축을 따라 단순한 선으로 작성됩니다. 'Curve.SweepAsSurface'는 경로 곡선을 따라 종단 곡선을 이동하여 표면을 작성합니다. 'cutEndOff' 매개변수는 스윕 표면의 끝 처리를 제어하는 부울입니다. 'true'로 설정하면 표면의 끝이 경로 곡선에 수직(법선)으로 절단되어 깔끔하고 평평한 말단이 생성됩니다. 'false'(기본값)로 설정하면 표면 끝은 자르기 없이 종단 곡선의 자연스러운 모양을 따르므로 경로 곡률에 따라 끝이 각지거나 고르지 않을 수 있습니다.
___
## 예제 파일

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

