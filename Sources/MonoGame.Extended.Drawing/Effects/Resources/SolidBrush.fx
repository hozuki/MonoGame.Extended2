#include "monogame_header.fxh"
#include "brush_def.fxh"

struct VS_IN {
    float2 Position : POSITION;
    float4 Color : COLOR;
};

struct PS_IN {
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

PS_IN vs(in VS_IN input) {
    PS_IN output = (PS_IN)0;
    
    // Invert Y (right-hand to left-hand; maths to GDI)
    input.Position.y = -input.Position.y;
    
    output.Position = mul(float4(input.Position, 0, 1), g_wvp);
    output.Color = input.Color;

    return output;
}

float4 ps(in PS_IN input) : SV_TARGET0 {
    float4 output = input.Color;

    output.a *= g_opacity;

    return output;
}

#include "footer.fxh"
