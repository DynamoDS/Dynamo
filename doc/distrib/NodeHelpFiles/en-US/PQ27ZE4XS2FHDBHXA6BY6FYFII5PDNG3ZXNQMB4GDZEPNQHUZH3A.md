<!--- Autodesk.DesignScript.Geometry.Surface.BySweep(profile, path, cutEndOff) --->
<!--- PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A --->
## In Depth
`Surface.BySweep (profile, path, cutEndOff)` creates a surface by sweeping an input curve along a specified path. The `cutEndOff` input controls whether to cut the end of the sweep and make it normal to the path.

In the example below, we use a sine curve in the y-direction as the profile curve. We rotate this curve by -90 degrees around the world z-axis to use as a path curve. Surface BySweep moves the profile curve along the path curve creating a surface.


___
## Example File

![Surface.BySweep](./PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A_img.jpg)