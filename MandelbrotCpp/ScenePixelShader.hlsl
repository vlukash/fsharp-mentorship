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

Texture2D       simpleTexture   : register(t0)  ;
SamplerState    simpleSampler   : register(s0)  ;

struct PixelShaderInput
{
    float4 pos  : SV_POSITION   ;
    float3 norm : NORMAL        ;
    float2 tex  : TEXCOORD0     ;
};

float4 main(PixelShaderInput input) : SV_TARGET
{
    // In the vertex shader, the normals were transformed into the world space,
    // so the lighting vector here will be relative to the world space.
    float3 lightDirection   = normalize(float3(1,1,1));
    float4 texelColor       = simpleTexture.Sample(simpleSampler, input.tex);
    float lightMagnitude    = 0.5f * saturate(dot( input.norm, -lightDirection)) + 0.5f;

    return texelColor * lightMagnitude;
}
