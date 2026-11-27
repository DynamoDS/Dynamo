# OpenGeometry Compatibility Inventory

This inventory is derived from Dynamo's checked-in public node documentation (`doc/distrib/NodeHelpFiles/en-US/Autodesk.DesignScript.Geometry.*.md`, 533 documented members across 43 types) and checked-in sample graphs (`doc/distrib/Samples/**/*.dyn`, 294 graphs across 14 locales; 21 distinct English sample graphs).

The pinned native modeling kernel is NVIDIA `usd-brep` commit `9701dd0cf4b8f4fe379290b3032e9407deb220df` (`v1.0.2`), built from `source/SMLib` and `source/SM_API` behind the versioned C ABI bridge in [bridge.h](native/bridge.h) (currently ABI v9).

## Sample Corpus Coverage

- **13 / 13** non-Excel, non-Python, non-intentional-warning English Dynamo samples under `doc/distrib/Samples/en-US/` run with **0 DummyNodes, 0 errors, and 0 warnings** in `SampleCorpusTests`:
  - `Basics_Basic01.dyn`, `Basics_Basic02.dyn`, `Basics_Basic03.dyn`
  - `Core_AttractorPoint.dyn`, `Core_CodeBlocks.dyn`, `Core_ListAtLevel.dyn`, `Core_ListLacing.dyn`, `Core_PassingFunctions.dyn`, `Core_RangeSyntax.dyn`
  - `Geometry_Curves.dyn`, `Geometry_Points.dyn`, `Geometry_Solids.dyn`, `Geometry_Surfaces.dyn`
- `Core_Strings.dyn` resolves all 53 nodes with 0 DummyNodes and 0 errors (its single `DSCore.String.ToNumber` warning is an intentional tutorial demonstration).

## Type and Slice Status

| Type | Public Docs | Status | Implemented Slice | Notes |
|---|---:|---|---|---|
| `Geometry` | 26 | Implemented (core) | Baseline + A–H | `Dispose`, `Tessellate` (`IGraphicItem` with per-vertex preview colors), `Translate`, `Rotate`, `Scale`, `Transform`, `Mirror`, `BoundingBox`, `DistanceTo`, `Split`, `Trim`, `Intersect`, `IntersectAll`, `DoesIntersect`, `ToJson`/`FromJson` (exact SMLib BRep/curve stream payloads, wired to `DSCore.Data.StringifyJSON`/`ParseJSON`/`Remember`); explicit `NotSupportedException` for SAT/SAB/SolidDef |
| `Point` | 14 | Implemented (core) | Baseline + A | `ByCoordinates` (2D & 3D), `Origin`, `ByCartesianCoordinates`, `X/Y/Z`, `Add`, `Subtract`, `AsVector`, `DistanceTo`, transforms |
| `Vector` | 26 | Implemented (core) | Slice A + H | `ByCoordinates`, `ByTwoPoints`, `XAxis/YAxis/ZAxis`, `Length`, `Normalized`, `Dot`, `Cross`, `Scale`, `Add`, `Subtract`, `Reverse`, `AngleWithVector`, `IsParallel`, `IsPerpendicular`, `AsPoint`, `ToJson`/`FromJson` |
| `UV` | 3 | Implemented | Slice A | `ByCoordinates`, `U`, `V` |
| `CoordinateSystem` | 28 | Implemented (core) | Slice A + H | `Identity`, `ByOrigin`, `ByOriginVectors`, `ByPlane`, `Origin`, `XAxis`, `YAxis`, `ZAxis`, `Determinant`, `IsUnscaledOrtho`, `Translate`, `Rotate`, `Scale`, `Inverse`, `ToJson`/`FromJson`, RGB axis `IGraphicItem` preview |
| `Plane` | 15 | Implemented (core) | Slice A | `XY`, `YZ`, `XZ`, `ByOriginNormal`, `ByOriginNormalXAxis`, `ByThreePoints`, `Origin`, `Normal`, `XAxis`, `YAxis`, `Offset`, transforms |
| `BoundingBox` | 11 | Implemented (core) | Slice A + H | `ByCorners`, `ByGeometry`, `MinPoint`, `MaxPoint`, `Contains`, `Intersects`, `ToCuboid`, `IsEmpty`, `ToJson`/`FromJson` |
| `Solid` | 19 | Implemented (core) | Baseline + A–D | `Volume`, `Area`, `Centroid`, `Difference`, `DifferenceAll`, `Union`, `ByUnion`, `Intersect`, `ByJoinedSurfaces`, `ByLoft` (cross-sections + guide curves), `ByRuledLoft`, `BySweep`, `ByRevolve`, `Fillet`, `Chamfer`, `ThinShell`, `Split(Plane)` |
| `Cuboid` | 4 | Implemented | Baseline + A | `ByLengths` (default, point, CS overloads), `ByCorners`, `Width`, `Length`, `Height` |
| `Cylinder` | 5 | Implemented | Baseline + A | `ByPointsRadius`, `ByRadiusHeight`, `Radius`, `Height`, `StartPoint`, `EndPoint` |
| `Sphere` | 5 | Implemented (core) | Slice A | `ByCenterPointRadius`, `CenterPoint`, `Radius` |
| `Cone` | 10 | Implemented (core) | Slice A | `ByPointsRadii`, `ByPointsRadius`, `ByCoordinateSystemHeightRadii`, `StartPoint`, `EndPoint`, `StartRadius`, `EndRadius`, `Height` |
| `Topology` | 3 | Implemented | Slice B | Base class for `Solid`, `Surface`, `PolySurface`; `Faces`, `Edges`, `Vertices` |
| `Face` | 3 | Implemented | Slice B | `SurfaceGeometry()`, `Edges`, `Vertices`, `Area`, `BoundingBox` |
| `Edge` | 4 | Implemented | Slice B | `CurveGeometry`, `StartVertex`, `EndVertex`, `AdjacentFaces`, `Length` |
| `Vertex` | 3 | Implemented | Slice B | `PointGeometry`, `AdjacentEdges`, `AdjacentFaces` |
| `Curve` | 55 | Implemented (core) | Slice B–G | `StartPoint`, `EndPoint`, `Length`, `IsClosed`, `PointAtParameter`, `TangentAtParameter`, `NormalAtParameter`, `PointAtSegmentLength`, `ParameterAtSegmentLength`, `ParameterAtPoint`, `Reverse`, `SplitByParameter`, `TrimByParameter`, `Offset`, `OffsetMany`, `Project`, `Patch`, `Extrude`, `ExtrudeAsSolid`, transforms, `Mirror`, preview |
| `Line` | 5 | Implemented (core) | Slice B + Corpus | `ByStartPointEndPoint`, `ByStartPointDirectionLength`, `ByBestFitThroughPoints`, `Direction` |
| `Circle` | 7 | Implemented (core) | Slice B | `ByCenterPointRadius`, `ByCenterPointRadiusNormal`, `ByPlaneRadius`, `ByThreePoints`, `CenterPoint`, `Radius`, `Normal` |
| `Arc` | 12 | Implemented (core) | Slice B | `ByCenterPointRadiusAngle`, `ByThreePoints`, `CenterPoint`, `Radius`, `StartAngle`, `SweepAngle`, `Normal` |
| `Ellipse` | 7 | Implemented | Slice D | `ByOriginRadii`, `ByOriginVectors`, `ByCoordinateSystemRadii`, `ByPlaneRadii`, `CenterPoint`, `MajorAxis`, `MinorAxis` |
| `EllipseArc` | 7 | Implemented | Slice D | `ByPlaneRadiiAngles`, `CenterPoint`, `MajorAxis`, `MinorAxis`, `Plane`, `StartAngle`, `SweepAngle` |
| `Helix` | 6 | Implemented | Slice D | `ByAxis`, `AxisPoint`, `AxisDirection`, `Pitch`, `Radius`, `Angle` |
| `NurbsCurve` | 12 | Implemented (core) | Slice B + Corpus + G | `ByPoints`, `ByControlPoints` (with degree & closure), `ByControlPointsWeightsKnots` (rational NURBS), `Degree` |
| `PolyCurve` | 13 | Implemented (core) | Slice B | `ByJoinedCurves`, `ByPoints`, `Curves()`, `NumberOfCurves`, full `Curve` overrides |
| `Polygon` | 8 | Implemented (core) | Slice B | `ByPoints`, `RegularPolygon`, `Points`, `Center()` |
| `Rectangle` | 3 | Implemented | Slice B | `ByWidthLength` (default, CS, Plane overloads), `ByCornerPoints`, `Width`, `Height` |
| `Surface` | 35 | Implemented (core) | Slice B–G | `ByPerimeterPoints`, `ByPatch`, `ByLoft` (with & without guide curves), `ByRuledLoft`, `BySweep`, `BySweep2Rails`, `ByRevolve`, `TrimWithEdgeLoops`, `Area`, `Perimeter`, `PerimeterCurves()`, `PointAtParameter`, `NormalAtParameter`, `Offset`, `Thicken`, transforms, `Mirror`, preview |
| `NurbsSurface` | 14 | Implemented (core) | Slice D | `ByPoints`, `ByControlPoints`, `DegreeU`, `DegreeV`, `NumControlPointsU`, `NumControlPointsV`, `IsPeriodicInU`, `IsPeriodicInV`, `IsRational`, `ControlPoints()`, `Weights()`, `UKnots()`, `VKnots()` |
| `PolySurface` | 15 | Implemented (core) | Slice B + Corpus | Inherits `Surface`; `ByJoinedSurfaces`, `BySolid`, `Surfaces()`, `SurfaceCount`, `EdgeCount`, `VertexCount`, `Solid()` |
| `Mesh` | 53 | Implemented (core) | Slice C + E + F + H | `ByGeometry`, `ByPointsFaceIndices`, `ByPointsIndexGroups`, `ByPointsIndices`, `ByVerticesAndIndices`, `Cuboid`, `Sphere`, `Cone`, `Plane`, `BooleanUnion`/`Difference`/`Intersection`, `PlaneCut`, `Intersect(Plane)`, `MakeWatertight`, `Nearest`, `Mirror`, `Edges`, `Explode`, `Smooth`, `ImportFile`/`ExportMeshes` (`.obj`/`.stl`/`.ply`), `ToJson`/`FromJson`, `Triangles`, `TriangleNormals`, `TriangleCentroids`, flat coordinate accessors, `Area`, `Volume`, transforms, preview |
| `IndexGroup` | 7 | Implemented | Slice C | `ByIndices` (3 & 4 indices), `A`, `B`, `C`, `D`, `Count` |
| `PanelSurface` / `PanelSurfaceBoundaryCondition` | 24 | Implemented (core) | Slice E + G | `PanelSurfaceBoundaryCondition` enum (`CoreNodes/ProtoGeometryHelper.cs` unguarded), `ByQuads`, `ByDiagonallySplitSquares`, `ByCrossSplitSquares`, `ByDiamonds`, `ByHexagons`, `ByStaggeredQuads`, `ByParallelograms`, `NumPanels`, `NumVertices`, `GetPoint`, `GetVertexIndex`, `GetNumPanelVertices`, `GetPanelPoints`, `GetPanelPolygon`, transforms, `Mirror`, preview |

## Tracked Compatibility Gaps

These areas are intentionally tracked separately from the core BRep/curve/surface/mesh slices and return explicit `NotSupportedException` diagnostics whenever an exposed signature reaches them:

1. **T-Splines (`Autodesk.DesignScript.Geometry.TSpline.*`, 10 types / 83 documented members)**
   - Autodesk-specific T-Spline subdivision surface engine (`TSplineSurface`, `TSplineTopology`, `TSplineVertex`, `TSplineEdge`, `TSplineFace`, `TSplineInitialSymmetry`, `TSplineReflection`, `TSplineUVNFrame`).
   - Not present in SMLib. Tracked as a separate gap; T-Spline experimental UI remains gated and unsupported in OpenGeometry.
2. **SAT / SAB / SolidDef binary and text interchange (`ExportToSAT`, `ImportFromSAT`, `SerializeAsSAB`, `DeserializeFromSAB`, `ToSolidDef`, `FromSolidDef`)**
   - SAT/SAB are proprietary ACIS/ASM persistence formats; `SolidDef` is Autodesk's internal interchange schema.
   - Exposed serialization/interchange signatures throw explicit `NotSupportedException` naming the unsupported format and explaining that OpenGeometry uses SMLib rather than ASM.
   - Open-format JSON persistence (`ToJson`/`FromJson`, `DSCore.Data.StringifyJSON`/`ParseJSON`/`Remember`) is implemented using exact in-memory SMLib binary BRep/curve streams plus open mesh formats (`.obj`/`.stl`/`.ply`) and native SMLib `.smb` export/import in `OpenGeometryBridge.dll`. Future CAD interchange formats (e.g., STEP / IGES / glTF / USD BrepArray) can be layered independently without re-introducing ASM.
3. **Downstream Package Binary Compatibility (Strong-Name / Assembly Identity / LibG Internals)**
   - Fork-owned `ProtoGeometry.dll` is intentionally unsigned (`PublicKeyToken=null`, `Product="OpenGeometry for Dynamo"`, `OpenGeometry.Kernel=SMLib`) so runtime audits can prove no Autodesk-built `ProtoGeometry.dll` or `LibG.Interface.dll` is loaded.
   - Zero-Touch graphs (`.dyn` files) bind by type/method name and `FunctionSignature` and survive cleanly across OpenGeometry.
   - Precompiled third-party `.dll` packages that hard-reference a strong-named Autodesk `ProtoGeometry.dll` or P/Invoke `LibG.Interface.dll` require recompilation or a dedicated assembly-binding shim (tracked separately).

## Design Rules

- Keep all NVIDIA `usd-brep` (`SMLib` / `SM_API`) interactions behind the C ABI bridge in `src/OpenGeometry/native/`.
- Keep Dynamo-specific semantics (Zero-Touch signatures, default values, coordinate/angle conventions, defensive copies for immutable graph semantics, `IGraphicItem` preview tessellation, and localized diagnostics) in C# under `src/OpenGeometry/ProtoGeometry/`.
- Never mutate or consume caller-owned BRep handles across immutable Dynamo operations (`Translate`, `Rotate`, `Scale`, `Union`, `Difference`, `Intersect`, `Fillet`, `Chamfer`, `ThinShell`, etc.).
- Every added slice requires analytic/exact verification tests, tolerance checks, topology/ordering invariants where applicable, preview tessellation tests, and updated `PublicAPI.Unshipped.txt`.
