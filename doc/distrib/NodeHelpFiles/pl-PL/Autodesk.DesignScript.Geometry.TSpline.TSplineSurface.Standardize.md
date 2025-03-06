## Informacje szczegółowe
Węzeł `TSplineSurface.Standardize` służy do standaryzowania powierzchni T-splajn.
Standaryzacja oznacza przygotowanie powierzchni T-splajn do konwersji NURBS i pociąga za sobą wydłużenie wszystkich punktów T do momentu oddzielenia ich od punktów gwiazdowych za pomocą co najmniej dwóch krzywych izometrycznych. Standaryzacja nie zmienia kształtu powierzchni, ale może spowodować dodanie punktów sterujących, aby spełnić wymagania dotyczące geometrii niezbędne do zapewnienia zgodności powierzchni z NURBS.

W poniższym przykładzie jedna z powierzchni w powierzchni T-splajn wygenerowanej za pomocą węzła `TSplineSurface.ByBoxLengths` zostaje podzielona na składowe.
Węzeł `TSplineSurface.IsStandard` sprawdza, czy powierzchnia jest standardowa — wynik jest negatywny.
Następnie za pomocą węzła `TSplineSurface.Standardize` ta powierzchnia zostaje ustandaryzowana. Powierzchnia wynikowa zostaje sprawdzona przy użyciu węzła `TSplineSurface.IsStandard`, co pozwala uzyskać potwierdzenie, że jest ona teraz standardowa.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Plik przykładowy

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
