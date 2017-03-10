float4 vPointParams = float4(4, 4, 0, 0);

//--------------------------------------------------------------------------------------
// VERTEX AND PIXEL SHADER INPUTS
//--------------------------------------------------------------------------------------
struct VSInputPS
{
	float4 p	: POSITION;
	float4 c	: COLOR;
};

struct GSInputPS
{
	float4 p	: POSITION;
	float4 c	: COLOR;
	float4 parameters  : COLOR1;
};

struct PSInputPS
{
	float4 p	: SV_POSITION;
	noperspective
		float3 t	: TEXCOORD;
	float4 c	: COLOR;
	float4 parameters : COLOR1;
};

void makeQuad(out float4 points[4], in float4 posA, in float w, in float h)
{
	// Bring A and B in window space
	float2 Aw = projToWindow(posA);
	float w2 = w * 0.5;
	float h2 = h * 0.5;

	// Compute the corners of the ribbon in window space
	float2 A1w = float2(Aw.x + w2, Aw.y + h2);
	float2 A2w = float2(Aw.x - w2, Aw.y + h2);
	float2 B1w = float2(Aw.x - w2, Aw.y - h2);
	float2 B2w = float2(Aw.x + w2, Aw.y - h2);

	// bring back corners in projection frame
	points[0] = windowToProj(A1w, posA.z, posA.w);
	points[1] = windowToProj(A2w, posA.z, posA.w);
	points[2] = windowToProj(B2w, posA.z, posA.w);
	points[3] = windowToProj(B1w, posA.z, posA.w);
}

//--------------------------------------------------------------------------------------
// POINTS SHADER
//-------------------------------------------------------------------------------------
GSInputPS VShaderPoints(GSInputPS input)
{
	GSInputPS output = (GSInputPS)0;
	output.p = input.p;
	output.parameters = input.parameters;

	bool isSelected = input.parameters.x;
	bool isIsolationMode = input.parameters.y;

	//set position into clip space	
	output.p = mul(output.p, mWorld);
	output.p = mul(output.p, mView);
	output.p = mul(output.p, mProjection);

	if (isSelected && !isIsolationMode) {
		output.c = lerp(vSelectionColor, input.c, 0.3);
	}
	else {
		output.c = input.c;
	}

	// unselected geometry is transparent under Isolate Selected Geometry mode
	if (isIsolationMode && !isSelected) {
		output.c.a = vTransparentLinePoint.a;
	}

	return output;
}

[maxvertexcount(4)]
void GShaderPoints(point GSInputPS input[1], inout TriangleStream<PSInputLS> outStream)
{
	PSInputPS output = (PSInputPS)0;

	float scale = 1.0;
	bool isSelected = input[0].parameters.x;
	if (isSelected) 
	{
		scale = 1.25;
	}

	float4 spriteCorners[4];
	makeQuad(spriteCorners, input[0].p, vPointParams.x * scale, vPointParams.y * scale);

	output.p = spriteCorners[0];
	output.c = input[0].c;
	output.t[0] = +1;
	output.t[1] = +1;
	output.t[2] = 1;
	output.parameters = input[0].parameters;
	outStream.Append(output);

	output.p = spriteCorners[1];
	output.c = input[0].c;
	output.t[0] = +1;
	output.t[1] = -1;
	output.t[2] = 1;
	output.parameters = input[0].parameters;
	outStream.Append(output);

	output.p = spriteCorners[2];
	output.c = input[0].c;
	output.t[0] = -1;
	output.t[1] = +1;
	output.t[2] = 1;
	output.parameters = input[0].parameters;
	outStream.Append(output);

	output.p = spriteCorners[3];
	output.c = input[0].c;
	output.t[0] = -1;
	output.t[1] = -1;
	output.t[2] = 1;
	output.parameters = input[0].parameters;
	outStream.Append(output);

	outStream.RestartStrip();
}

float4 PShaderPoints(PSInputPS input) : SV_Target
{
	if (vPointParams[2] == 1)
	{
		float len = length(input.t);
		if (len > 1.4142) discard;
	}
	else if (vPointParams[2] == 2)
	{
		float arrowScale = 1 / (vPointParams[3] + (input.t[2] > 0.9)*(-input.t[2] + 1) * 10);
		float alpha = min(abs(input.t[0]), abs(input.t[1]));
		float dist = arrowScale * alpha;
		alpha = exp2(-4 * dist*dist*dist*dist);
		if (alpha < 0.1) discard;
	}

return input.c;
}

//--------------------------------------------------------------------------------------
// Techniques
//-------------------------------------------------------------------------------------
technique11 RenderPoints
{
	pass P0
	{
		//SetDepthStencilState( DSSDepthLess, 0 );
		//SetDepthStencilState( DSSDepthLessEqual, 0 );
		//SetRasterizerState	( RSLines );
		//SetRasterizerState( RSFillBiasBack );
		//SetBlendState		( BSBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		SetVertexShader(CompileShader(vs_4_0, VShaderPoints()));
		SetHullShader(NULL);
		SetDomainShader(NULL);
		SetGeometryShader(CompileShader(gs_4_0, GShaderPoints()));
		SetPixelShader(CompileShader(ps_4_0, PShaderPoints()));
	}
}