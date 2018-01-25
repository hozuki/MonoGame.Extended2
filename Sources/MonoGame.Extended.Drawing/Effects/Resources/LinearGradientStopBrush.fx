#include "monogame_header.fxh"
#include "brush_def.fxh"
#include "gradient_def.fxh"

cbuffer cbLinearGradientBrush {
    float2 startPoint;
    float2 endPoint;
    float2 direction;
}

struct VS_IN {
    float2 Position : POSITION0;
};

struct PS_IN {
    float4 Position : SV_POSITION;
};

PS_IN vs(in VS_IN input) {
    PS_IN output = (PS_IN)0;

    output.Position = mul(float4(input.Position, 0, 1), g_wvp);

    return output;
}

float4 ps(in PS_IN input) : COLOR {
    float4 output = float4(0);
    bool colorSet = false;

    if (!colorSet) {
        for (int i = 0; i < g_numGradientStop; ++i) {           
            GradientStop gs = g_gradientStops[i];

            output = gs.Color;
            colorSet = true;
        }
    }

    if (!colorSet) {
        output = g_gradientStops[g_numGradientStop - 1].Color;
    }

    output.a *= g_opacity;

    return output;
}

#include "footer.fxh"
