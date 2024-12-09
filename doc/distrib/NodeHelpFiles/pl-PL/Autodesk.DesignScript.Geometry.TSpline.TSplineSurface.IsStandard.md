## Informacje szczegółowe
Powierzchnia T-splajn jest standardowa, gdy wszystkie punkty T są oddzielone od punktów gwiazdowych za pomocą co najmniej dwóch krzywych izometrycznych. Standaryzacja jest niezbędna do przekształcenia powierzchni T-splajn w powierzchnię NURBS.

W poniższym przykładzie jedna z powierzchni w powierzchni T-splajn wygenerowanej za pomocą węzła `TSplineSurface.ByBoxLengths` zostaje podzielona na składowe. Węzeł `TSplineSurface.IsStandard` sprawdza, czy powierzchnia jest standardowa — wynik jest negatywny.
Następnie za pomocą węzła `TSplineSurface.Standardize` ta powierzchnia zostaje ustandaryzowana. Wprowadzone zostają nowe punkty sterowania bez zmieniania kształtu powierzchni. Powierzchnia wynikowa zostaje sprawdzona przy użyciu węzła `TSplineSurface.IsStandard`, co pozwala uzyskać potwierdzenie, że jest ona teraz standardowa.
Za pomocą węzłów `TSplineFace.UVNFrame` i `TSplineUVNFrame.Position` zostaje wyróżniona podzielona na składowe powierzchnia danej powierzchni.
___
## Plik przykładowy

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
