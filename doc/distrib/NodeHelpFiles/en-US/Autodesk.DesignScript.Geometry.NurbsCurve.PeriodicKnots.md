## In Depth
PeriodicKnots returns the knot vector of a periodic NURBS curve in its *periodic* (unclamped) form. Knots returns the knot vector in the *clamped* form. Both arrays have the **same length** (for *n*+1 control points and degree *p*, the knot vector has *n*+*p*+2 values); they are two different representations of the same curve.

A periodic NURBS curve is a smooth, closed loop without endpoints. In the clamped form, the first and last *p*+1 knots are repeated so the curve is pinned to the parameter domain; in the periodic form, the knot vector is unclamped (no such end repetition), and the knot *differences* repeat at the start and end of the sequence. That structure gives smooth closure (C^(p-1) at the join). The periodic form is the one required for correct export or round-trip workflows with systems (e.g. Alias) that expect the periodic definition.

The example graph illustrates this difference on a single periodic NURBS curve. The curve is built with **NurbsCurve.ByControlPointsWeightsKnots** from a list of points, uniform weights, a knot vector, and degree (e.g. 5). From that curve, **NurbsCurve.Knots** and **NurbsCurve.PeriodicKnots** are both evaluated; Watch nodes show the two knot arrays so you can compare length and values. The graph also uses **NurbsCurve.ControlPoints** (displayed in red via **GeometryColor.ByGeometryColor**), **NurbsCurve.PeriodicControlPoints**, and **NurbsCurve.IsPeriodic** for context. When run, the graph shows that both knot arrays have the same length but different values: Knots is the clamped form (repeated knots at the ends), and PeriodicKnots is the unclamped form with the repeating difference pattern that defines the curve's periodicity.

This node is particularly useful when you need to export periodic curves to other systems, analyze the underlying mathematical structure of periodic NURBS, or ensure round-trip accuracy when exchanging geometric data with applications like Alias.
___
## Example File

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)

