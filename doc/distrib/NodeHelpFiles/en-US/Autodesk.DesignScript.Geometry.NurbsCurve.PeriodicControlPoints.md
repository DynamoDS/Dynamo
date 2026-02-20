## In Depth
PeriodicControlPoints returns the control points of a periodic NURBS curve in its *periodic* form. ControlPoints returns the control points in the *clamped* (open) form. Both arrays have the **same number** of points; they are two different representations of the same curve, not a base set plus extra points.

In the periodic representation, the curve’s closure and smoothness are encoded by a wraparound rule: the **last *p* control points equal the first *p* control points**, where *p* is the degree of the curve. This gives C^(p-1) continuity at the join. The clamped form uses a different knot vector and control-point layout (with no such wraparound), so the actual point positions in the two arrays differ. Neither representation is a superset of the other—they are alternative parameterizations. The periodic form is required for correct export, modification, or round-trip workflows with systems (e.g. Alias) that expect the periodic definition.

The example graph illustrates this difference on a single periodic NURBS curve. The curve is built with **NurbsCurve.ByControlPointsWeightsKnots** from a list of points, uniform weights, a knot vector, and degree (e.g. 5). From that curve, **NurbsCurve.ControlPoints** and **NurbsCurve.PeriodicControlPoints** are both evaluated; Watch nodes show the two lists so you can compare count and values. The ControlPoints are also passed to **GeometryColor.ByGeometryColor** (with a red color) so they appear in the background preview. The graph also uses **NurbsCurve.Knots**, **NurbsCurve.PeriodicKnots**, and **NurbsCurve.IsPeriodic** to show the curve’s periodic state. When run, the graph shows that both arrays have the same length but different point positions: ControlPoints is the clamped form, and PeriodicControlPoints satisfies the wraparound rule (last *p* points equal the first *p*, where *p* is the degree).

This node is critical when working with periodic curves in data exchange scenarios, particularly with applications like Alias, or when you need to analyze or modify the complete mathematical definition of a closed NURBS curve while maintaining its periodic properties.
___
## Example File

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)

