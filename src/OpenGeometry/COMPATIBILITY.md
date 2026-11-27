# OpenGeometry Compatibility Inventory

This inventory is derived from Dynamo's checked-in public node documentation (`doc/distrib/NodeHelpFiles/en-US/Autodesk.DesignScript.Geometry.*.md`, 533 documented members across 43 types) and checked-in sample graphs (`doc/distrib/Samples/**/*.dyn`, 294 graphs).

The pinned native modeling kernel is NVIDIA `usd-brep` commit `9701dd0cf4b8f4fe379290b3032e9407deb220df` (`v1.0.2`), built from `source/SMLib` and `source/SM_API` behind the versioned C ABI bridge in [bridge.h](native/bridge.h).

## Type and Slice Status

| Type | Public Docs | Status | Target Slice | Notes |
|---|---:|---|---|---|
| `Geometry` | 26 | Partial | Baseline + Slice A/C | `Dispose`, `IGraphicItem.Tessellate`, rigid transforms (`Translate`, `Rotate`, `Scale`, `Transform`), `BoundingBox`, `Split`; explicit `NotSupportedException` for SAT/SAB/SolidDef |
| `Point` | 14 | Implemented (core) | Baseline + Slice A | `ByCoordinates` (2D & 3D), `Origin`, `ByCartesianCoordinates`, `X/Y/Z`, `Add`, `Subtract`, `AsVector`, `DistanceTo`, `Translate`, `Rotate`, `Scale`, `Transform` |
| `Vector` | 26 | Planned → Slice A | Slice A | `ByCoordinates`, `ByTwoPoints`, `XAxis/YAxis/ZAxis`, `Length`, `Normalized`, `Dot`, `Cross`, `Scale`, `Add`, `Subtract`, `Reverse`, `AngleWithVector`, `IsParallel`, `IsPerpendicular`, `AsPoint` |
| `UV` | 3 | Planned → Slice A | Slice A | `ByCoordinates`, `U`, `V` |
| `CoordinateSystem` | 28 | Planned → Slice A | Slice A | `Identity`, `ByOrigin`, `ByOriginVectors`, `ByPlane`, `Origin`, `XAxis`, `YAxis`, `ZAxis`, `Translate`, `Rotate`, `Scale`, `Inverse` |
| `Plane` | 15 | Planned → Slice A | Slice A | `XY`, `YZ`, `XZ`, `ByOriginNormal`, `ByOriginNormalXAxis`, `ByThreePoints`, `Origin`, `Normal`, `XAxis`, `YAxis`, `Offset` |
| `BoundingBox` | 11 | Planned → Slice A | Slice A | `ByCorners`, `ByGeometry`, `MinPoint`, `MaxPoint`, `Contains`, `Intersects`, `ToCuboid`, `IsEmpty` |
| `Solid` | 19 | Partial → Slice A/B/C | Baseline + Slice A/C | `Volume`, `Area`, `Centroid`, `Difference`, `DifferenceAll`, `Union`, `ByUnion`, `Intersect`, `ThinShell`, `Fillet`, `Chamfer`, `ByLoft`, `BySweep`, `ByJoinedSurfaces` |
| `Cuboid` | 4 | Implemented (core) | Baseline + Slice A | `ByLengths` (origin & world defaults), `ByCorners`, `Width`, `Length`, `Height` |
| `Cylinder` | 5 | Implemented (core) | Baseline + Slice A | `ByPointsRadius`, `ByRadiusHeight`, `Radius`, `Height`, `StartPoint`, `EndPoint` |
| `Sphere` | 5 | Planned → Slice A | Slice A | `ByCenterPointRadius`, `ByFourPoints`, `CenterPoint`, `Radius` |
| `Cone` | 10 | Planned → Slice A | Slice A | `ByPointsRadii`, `ByPointsRadius`, `ByCoordinateSystemHeightRadii`, `StartPoint`, `EndPoint`, `StartRadius`, `EndRadius`, `Height` |
| `Topology` | 3 | Planned → Slice B | Slice B | Base class for `Solid` and `Surface`; `Faces`, `Edges`, `Vertices` |
| `Face` | 3 | Planned → Slice B | Slice B | `SurfaceGeometry`, `Edges`, `Vertices` |
| `Edge` | 4 | Planned → Slice B | Slice B | `CurveGeometry`, `StartVertex`, `EndVertex`, `AdjacentFaces` |
| `Vertex` | 3 | Planned → Slice B | Slice B | `PointGeometry`, `AdjacentEdges`, `AdjacentFaces` |
| `Curve` | 55 | Planned → Slice B/C | Slice B/C | `StartPoint`, `EndPoint`, `Length`, `IsClosed`, `PointAtParameter`, `TangentAtParameter`, `NormalAtParameter`, `PointAtSegmentLength`, `SplitByParameter`, `TrimByParameter`, `Extrude`, `ExtrudeAsSolid`, `Patch`, `Reverse` |
| `Line` | 5 | Planned → Slice B | Slice B | `ByStartPointEndPoint`, `ByStartPointDirectionLength`, `Direction` |
| `Circle` | 7 | Planned → Slice B | Slice B | `ByCenterPointRadius`, `ByCenterPointRadiusNormal`, `ByPlaneRadius`, `ByThreePoints`, `CenterPoint`, `Radius`, `Normal` |
| `Arc` | 12 | Planned → Slice B | Slice B | `ByCenterPointRadiusAngle`, `ByThreePoints`, `ByCenterPointStartPointEndPoint`, `CenterPoint`, `Radius`, `StartAngle`, `SweepAngle` |
| `NurbsCurve` | 12 | Planned → Slice B | Slice B | `ByPoints`, `ByControlPoints`, `Degree`, `ControlPoints`, `Knots`, `Weights`, `IsPeriodic` |
| `PolyCurve` | 13 | Planned → Slice B | Slice B | `ByJoinedCurves`, `ByPoints`, `Curves`, `NumberOfCurves` |
| `Polygon` | 8 | Planned → Slice B | Slice B | `ByPoints`, `RegularPolygon`, `Points`, `Center` |
| `Rectangle` | 3 | Planned → Slice B | Slice B | `ByWidthLength`, `ByCornerPoints`, `Width`, `Length` |
| `Ellipse` / `EllipseArc` / `Helix` | 20 | Planned | Future | Analytic/NURBS curve extensions |
| `Surface` | 35 | Planned → Slice B/C | Slice B/C | `Area`, `PerimeterCurves`, `PointAtParameter`, `NormalAtParameter`, `ByPatch`, `ByLoft`, `BySweep`, `ByPerimeterPoints`, `Thicken`, `Offset`, `TrimWithEdgeLoops` |
| `NurbsSurface` | 14 | Planned | Future | Control-point NURBS surface construction and queries |
| `PolySurface` | 15 | Planned → Slice B/C | Slice B/C | `ByJoinedSurfaces`, `Surfaces`, `Solid`, `SurfaceCount`, `UnconnectedBoundaries` |
| `Mesh` | 53 | Planned → Slice C | Slice C | `ByPointsFaceIndices`, `VertexPositions`, `VertexNormals`, `FaceIndices`, `Triangles`, `Area`, `Volume`, `Translate`, `Scale` |
| `IndexGroup` | 7 | Planned → Slice C | Slice C | `ByIndices` (3 & 4 indices), `A`, `B`, `C`, `D`, `Count` |
| `PanelSurface` / `PanelSurfaceBoundaryCondition` | 24 | Deferred | Future | Paneling domain types |

## Tracked Compatibility Gaps

These areas are intentionally tracked separately from the core BRep/curve/surface/mesh slices and return explicit `NotSupportedException` diagnostics whenever an exposed signature reaches them:

1. **T-Splines (`Autodesk.DesignScript.Geometry.TSpline.*`, 10 types / 83 documented members)**
   - Autodesk-specific T-Spline subdivision surface engine (`TSplineSurface`, `TSplineTopology`, `TSplineVertex`, `TSplineEdge`, `TSplineFace`, `TSplineInitialSymmetry`, `TSplineReflection`, `TSplineUVNFrame`).
   - Not present in SMLib. Tracked as a separate gap; T-Spline experimental UI remains gated and unsupported in OpenGeometry.
2. **SAT / SAB / SolidDef binary and text interchange (`ExportToSAT`, `ImportFromSAT`, `SerializeAsSAB`, `DeserializeFromSAB`, `ToSolidDef`, `FromSolidDef`)**
   - SAT/SAB are proprietary ACIS/ASM persistence formats; `SolidDef` is Autodesk's internal interchange schema.
   - Exposed serialization/interchange signatures throw explicit `NotSupportedException` naming the unsupported format and explaining that OpenGeometry uses SMLib rather than ASM.
   - Future open-format interchange (e.g., STEP / IGES / OBJ / glTF / USD BrepArray) can be layered independently without re-introducing ASM.
3. **Downstream Package Binary Compatibility (Strong-Name / Assembly Identity / LibG Internals)**
   - Fork-owned `ProtoGeometry.dll` is intentionally unsigned (`PublicKeyToken=null`, `Product="OpenGeometry for Dynamo"`, `OpenGeometry.Kernel=SMLib`) so runtime audits can prove no Autodesk-built `ProtoGeometry.dll` or `LibG.Interface.dll` is loaded.
   - Zero-Touch graphs (`.dyn` files) bind by type/method name and `FunctionSignature` and survive cleanly across OpenGeometry.
   - Precompiled third-party `.dll` packages that hard-reference a strong-named Autodesk `ProtoGeometry.dll` or P/Invoke `LibG.Interface.dll` require recompilation or a dedicated assembly-binding shim (tracked separately).

## Design Rules

- Keep all NVIDIA `usd-brep` (`SMLib` / `SM_API`) interactions behind the C ABI bridge in `src/OpenGeometry/native/`.
- Keep Dynamo-specific semantics (Zero-Touch signatures, default values, coordinate/angle conventions, defensive copies for immutable graph semantics, `IGraphicItem` preview tessellation, and localized diagnostics) in C# under `src/OpenGeometry/ProtoGeometry/`.
- Never mutate or consume caller-owned BRep handles across immutable Dynamo operations (`Translate`, `Rotate`, `Scale`, `Union`, `Difference`, `Intersect`, `Fillet`, `Chamfer`, `ThinShell`, etc.).
- Every added slice requires analytic/exact verification tests, tolerance checks, topology/ordering invariants where applicable, preview tessellation tests, and updated `PublicAPI.Unshipped.txt`.
