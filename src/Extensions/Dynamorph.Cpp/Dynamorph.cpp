// Dynamorph.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Dynamorph.h"


// This is an example of an exported variable
DYNAMORPH_API int nDynamorph=0;

// This is an example of an exported function.
DYNAMORPH_API int fnDynamorph(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see Dynamorph.h for the class definition
CDynamorph::CDynamorph()
{
	return;
}
