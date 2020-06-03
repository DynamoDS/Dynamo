#ifndef VSPOINTLINEDEFAULT_HLSL
#define VSPOINTLINEDEFAULT_HLSL
#define POINTLINE
#include"Common.hlsl"
#include"DataStructs.hlsl"
#pragma pack_matrix( row_major )

GSInputPS main(VSInputPS input)
{
	GSInputPS output;

	output.p = input.p;

	//set position into clip space	
	output.p = mul(output.p, mWorld);
	output.p = mul(output.p, mView);
	output.p = mul(output.p, mProjection);

	output.c = input.c * pColor;
	
	return output;
}

#endif