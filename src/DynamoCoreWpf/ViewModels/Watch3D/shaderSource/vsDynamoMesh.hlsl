#ifndef VSMESHDEFAULT_HLSL
#define VSMESHDEFAULT_HLSL
#define MESH
#include"Common.hlsl"
#include"DataStructs.hlsl"
#include"DynamoCommonStructures.hlsl"
#pragma pack_matrix( row_major )

PSInputCustom main(VSInput input)
{
    PSInputCustom output;

    //our flags are packed in this order:
    /* TODO update.
    None = 0,
        IsFrozen = 1,
        IsSelected = 2,
        IsIsolated = 4,
        IsSpecialRenderPackage = 8,
        //TODO do we need this flag?
        HasTransparency = 16,
        //TODO add vertex colors
        //TODO add flat shade?
        */

    uint flags = int(vParams.x);
    bool isFrozen = flags & 1;
    bool isSelected = flags & 2;
    bool isIsolated = flags & 4;
    bool isSpecialRenderPackage = flags & 8;
    bool requiresPerVertexColoration = flags & 16;
        
    float4 inputp;
    if (isSelected || requiresPerVertexColoration)
    {
        // Nudge the vertex out slightly along its normal.
        float3 nudge = normalize(input.n) * 0.0001;
        inputp = float4(input.p.x + nudge.x, input.p.y + nudge.y, input.p.z + nudge.z, input.p.w);
    }
    else
    {
        inputp = input.p;
    }

    //TODO shader used to set instance matrix here....do we need to do this still?

    //
    

    float3 inputn = input.n;
    float3 inputt1 = input.t1;
    float3 inputt2 = input.t2;

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

    //send our flags through to the frag shader
    output.customParams.x = vParams.x;

    return output;
}

#endif