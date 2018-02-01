#include "monogame_header.fxh"
#include "brush_def.fxh"

cbuffer cbSolidColorBrush {
    float4 g_color;
};

struct VS_IN {
    float2 Position : POSITION;
};

struct PS_IN {
    float4 PositionT : SV_POSITION;
};

PS_IN vs(in VS_IN input) {
    PS_IN output = (PS_IN)0;
    
    output.PositionT = mul(float4(input.Position, 0, 1), g_wvp);

    return output;
}

float4 ps(in PS_IN input) : SV_TARGET {
    float4 output = g_color;

    output.a *= g_opacity;

    return output;
}

technique SolidColorBrush {
    pass {
		VertexShader = compile VS_SHADERMODEL vs();
		PixelShader = compile PS_SHADERMODEL ps();
    }
};
