#ifndef VSMESHDEFAULT_HLSL
#define VSMESHDEFAULT_HLSL
#define MESH
#include"Common.hlsl"
#include"DataStructs.hlsl"
#pragma pack_matrix( row_major )

PSInput main(VSInput input)
{
    PSInput output;

    //our flags are packed in this order:
    /* 
        None = 0,
        IsFrozen = 1,
        IsSelected = 2,
        IsIsolated = 4,
        IsSpecialRenderPackage = 8,
        HasTransparency = 16, //not used
        RequiresPerVertexColoration = 32
        FlatShade =64 //not used
        */

    uint flags = int(vParams.x);
    bool isSelected = flags & 2;
    bool requiresPerVertexColoration = flags & 32;
     
    float4 inputp;
    float3 nudge = normalize(input.n) * 0.0001;

    inputp = input.p;
    if (requiresPerVertexColoration)
    {
        // Nudge the vertex out slightly along its normal a tiny bit.
        inputp = float4(inputp.x + nudge.x, inputp.y + nudge.y, inputp.z + nudge.z, inputp.w);
    }
    if (isSelected)
    {
        // Nudge the vertex out slightly along its normal a bit more.
        nudge  = nudge * 100;
        inputp = float4(inputp.x + nudge.x, inputp.y + nudge.y, inputp.z + nudge.z, inputp.w);
    }
    

    //set position into world space
    output.p = mul(inputp, mWorld);
    output.wp = output.p;
    //set position into clip space	
    output.p = mul(output.p, mViewProjection);

    //set texture coords and color
    output.t = input.t;
    output.c = input.c;

    //set normal for interpolation	
    output.n = normalize(mul(input.n, (float3x3)mWorld));

    return output;
}

#endif