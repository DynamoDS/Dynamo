#ifndef PSPOINTLINEDEFAULT_HLSL
#define PSPOINTLINEDEFAULT_HLSL
#define POINTLINE
#include"Common.hlsl"
#include"CommonBuffers.hlsl"

float4 main(PSInputPS input) : SV_Target
{
	float4 vSelectionColor = float4(0.0, 0.62, 1.0, 1.0);
	//reusing this param.
	int flags = int(fadeNearDistance);
	bool isFrozen = flags & 1;
	bool isSelected = flags & 2;
	bool isIsolated = flags & 4;
	bool isSpecialRenderPackage = flags & 8;

	// if this is a special render package - it should render with the material colors, ambient light
		// and not be directionally lit.
	if (isSpecialRenderPackage) {
		return input.c;
	}

	/// set diffuse alpha if selected or normal
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
		I.a = .1f;
	}

	if (isSelected && !isIsolated)
	{
		I = lerp(vSelectionColor, I, 0.3);
	}

	return I;
}

#endif