// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A
// copy of the license can be found in the License.html file at the root of this distribution.
// If you cannot locate the  Microsoft Public License, please send an email to
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------

// A constant buffer that stores the three basic column-major matrices for composing geometry.
cbuffer ModelViewProjectionConstantBuffer : register(b0)
{
	matrix model        ;
	matrix view         ;
	matrix projection   ;
};

// Per-vertex data used as input to the vertex shader.
struct VertexShaderInput
{
    float3 pos  : POSITION  ;
    float3 norm : NORMAL    ;
    float2 tex  : TEXCOORD0 ;
};

// Per-pixel color data passed through the pixel shader.
struct PixelShaderInput
{
    float4 pos  : SV_POSITION   ;
    float3 norm : NORMAL        ;
    float2 tex  : TEXCOORD0     ;
};

// Simple shader to do vertex processing on the GPU.
PixelShaderInput main(VertexShaderInput input)
{
    PixelShaderInput vertexShaderOutput;
    float4 pos = float4(input.pos, 1.0f);

    // Transform the vertex position into projection space.
    pos = mul(pos, model);
    pos = mul(pos, view);
    pos = mul(pos, projection);
    vertexShaderOutput.pos = pos;

    // Pass through the texture coordinate without modification.
    vertexShaderOutput.tex = input.tex;

    // Transform the normal into world space to allow world-space lighting.
    float4 norm = float4(normalize(input.norm), 0.0f);
    norm = mul(norm, model);
    vertexShaderOutput.norm = normalize(norm.xyz);
    return vertexShaderOutput;
}

