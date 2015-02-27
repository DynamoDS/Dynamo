##Arc By Start Mid End
###Description
Creates a geometric arc given start, middle and end points in XYZ.

###Inputs
  * **start** *(XYZ)* - Start XYZ
  * **mid** *(XYZ)* - XYZ on Curve
  * **end** *(XYZ)* - End XYZ

###Output
  * None


##Arc by Ctr Pt
###Description
Creates a geometric arc given a center point and two end parameters. Start and End Values may be between 0 and 2*PI in Radians

###Inputs
  * **center** *(XYZ)* - Center XYZ
  * **radius** *(double)* - Radius
  * **start** *(double)* - Start Param
  * **end** *(double)* - End Param

###Output
  * None


##Circle
###Description
Creates a geometric circle.

###Inputs
  * **start** *(XYZ)* - Start XYZ
  * **rad** *(double)* - Radius

###Output
  * None


##Curve From Curve Ele
###Description
Takes in a Model Curve and Extracts Geometry Curve

###Inputs
  * **mc** *(object)* - Model Curve Element

###Output
  * None


##Ellipse
###Description
Creates a geometric ellipse.

###Inputs
  * **center** *(XYZ)* - Center XYZ
  * **radX** *(double)* - Major Radius
  * **radY** *(double)* - Minor Radius

###Output
  * None


##Ellipse Arc
###Description
Creates a geometric elliptical arc. Start and End Values may be between 0 and 2*PI in Radians

###Inputs
  * **center** *(XYZ)* - Center XYZ
  * **radX** *(double)* - Major Radius
  * **radY** *(double)* - Minor Radius
  * **start** *(double)* - Start Param
  * **end** *(double)* - End Param

###Output
  * None


##Hermite Spline
###Description
Creates a geometric hermite spline.



###Output
  * None


##Line
###Description
Creates a geometric line.

###Inputs
  * **start** *(XYZ)* - Start XYZ
  * **end** *(XYZ)* - End XYZ

###Output
  * None


##Plane
###Description
Creates a geometric plane.



###Output
  * None


##Sketch Plane
###Description
Creates a geometric sketch plane.



###Output
  * None


##Transform Crv
###Description
Returns the curve (c) transformed by the transform (t).



###Output
  * None
