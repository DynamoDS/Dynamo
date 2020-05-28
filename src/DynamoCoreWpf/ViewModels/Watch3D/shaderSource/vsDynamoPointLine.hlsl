#ifndef VSPOINTLINEDEFAULT_HLSL
#define VSPOINTLINEDEFAULT_HLSL
#define POINTLINE
#include"Common.hlsl"
#include"DataStructs.hlsl"
#pragma pack_matrix( row_major )

PSInputPS main(VSInputPS input)
{
    PSInputPS output;

	output.p = input.p;
	//set position into clip space	
	output.p = mul(output.p, mWorld);
	output.p = mul(output.p, mView);
	output.p = mul(output.p, mProjection);

	output.c = input.c;
	
	return output;
}

#endif