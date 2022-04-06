#ifndef PSPOINTLINEDEFAULT_HLSL
#define PSPOINTLINEDEFAULT_HLSL
#define POINTLINE
#include"Common.hlsl"
#include"CommonBuffers.hlsl"

float4 main(PSInputPS input) : SV_Target
{
	float4 vSelectionColor = float4(0.023529, 0.588235, 0.843137, 1.0);

	//our flags are packed in this order:
  /*
	  None = 0,
	  IsFrozen = 1,
	  IsSelected = 2,
	  IsIsolated = 4,
	  IsSpecialRenderPackage = 8,
	  HasTransparency = 16, //not used
	  RequiresPerVertexColoration = 32
	  FlatShade = 64 //not used
	  */

	//reusing this param for state data.
	int flags = int(fadeNearDistance);
	bool isFrozen = flags & 1;
	bool isSelected = flags & 2;
	bool isIsolated = flags & 4;

	//if the figure is a circle,
	//clip pixels outside relative radius.
	 if (pfParams[2] == 1)
    {
        float len = length(input.t);
        if (len > 1.4142)
            discard;
    }

	float4 I = input.c;

	//if frozen half alpha
	if (isFrozen)
	{
		I.a = .5f;
	}
	//if isolated - alpha should always be low - 
	//overriding other alpha values (except if selected)
	if (isIsolated && !isSelected)
	{
		I.a = .2f;
	}

	if (isSelected && !isIsolated)
	{
		I = lerp(vSelectionColor, I, 0.3);
	}

	return I;
}

#endif