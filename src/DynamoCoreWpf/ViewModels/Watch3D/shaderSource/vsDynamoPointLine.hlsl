#ifndef VSPOINTLINEDEFAULT_HLSL
#define VSPOINTLINEDEFAULT_HLSL
#define POINTLINE
#include"Common.hlsl"
#include"DataStructs.hlsl"
#pragma pack_matrix( row_major )

GSInputPS main(VSInputPS input)
{
	GSInputPS output;

	float4 inputp = input.p;

    // compose instance matrix
    if (bHasInstances)
    {
        matrix mInstance =
        {
            input.mr0,
            input.mr1,
            input.mr2,
            input.mr3
        };
        inputp = mul(inputp, mInstance);
    }

	//set position into clip space	
	output.p = mul(inputp, mWorld);
	output.p = mul(output.p, mView);
	output.p = mul(output.p, mProjection);

	output.c = input.c * pColor;
	
	return output;
}

#endif