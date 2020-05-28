#ifndef VSPOINTDEFAULT_HLSL
#ifndef PSPOINTLINEDEFAULT_HLSL
#define PSPOINTLINEDEFAULT_HLSL
#define POINTLINE
#include"Common.hlsl"
#include"DataStructs.hlsl"
#pragma pack_matrix( row_major )

float4 main(PSInputPS input)
{
	float4 vSelectionColor = float4(0.0, 0.62, 1.0, 1.0);

    bool isFrozen = bool(pfParams.x);
	bool isSelected = bool(pfParams.y);
	bool isIsolated = bool(pfParams.z);
	bool isSpecialRenderPackage = bool(pfParams.w);
        
	// if this is a special render package - it should render with the material colors, ambient light
	// and not be directionally lit.
	if (isSpecialRenderPackage) {
		return vMaterialDiffuse + vMaterialEmissive + vMaterialAmbient * vLightAmbient;
	}

	/// set diffuse alpha if selected or normal
	I.a = vMaterialDiffuse.a;

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