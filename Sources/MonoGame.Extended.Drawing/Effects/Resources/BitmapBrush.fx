#include "monogame_header.fxh"
#include "brush_def.fxh"

cbuffer cbBitmapBrush {
    Texture2D g_texture;
}

SamplerState sam {
};

struct VS_IN {
    float2 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};

struct PS_IN {
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

PS_IN vs(in VS_IN input) {
    PS_IN output = (PS_IN)0;

    output.Position = mul(float4(input.Position, 0, 1), g_wvp);
    output.TexCoord = input.TexCoord;

    return output;
}

float4 ps(in PS_IN input) {
    float4 output = g_texture.Sample(sam, input.TexCoord);

    output.a *= g_opacity;

    return output;
}

#include "footer.fxh"
