#include "monogame_header.fxh"
#include "brush_def.fxh"
#include "gradient_def.fxh"

cbuffer cbLinearGradientBrush {
    float2 startPoint;
    float2 endPoint;
}

struct VS_IN {
    float2 Position : POSITION;
};

struct PS_IN {
    float4 PositionT : SV_POSITION;
    float2 Position : COLOR0;
};

PS_IN vs(in VS_IN input) {
    PS_IN output = (PS_IN)0;

    output.PositionT = mul(float4(input.Position, 0, 1), g_wvp);
    output.Position = input.Position;

    return output;
}

// Basic ideas: https://github.com/Rokotyan/Linear-Gradient-Shader
float4 ps(in PS_IN input, GRADIENT_PARAMS_DECL) {
    float alpha = atan2(-(endPoint.y - startPoint.y), endPoint.x - startPoint.x);
    float startRotation = startPoint.x * cos(alpha) - startPoint.y * sin(alpha);
    float endRotation = endPoint.x * cos(alpha) - endPoint.y * sin(alpha);
    float grad = endRotation - startRotation;

    float loc = input.Position.x * cos(alpha) - input.Position.y * sin(alpha);

    if (extendMode == EXTEND_MODE_CLAMP) {
        // Do nothing.
    } else if (extendMode == EXTEND_MODE_WRAP || extendMode == EXTEND_MODE_MIRROR) {
        float times = 0.0f;
        float delta = endRotation - startRotation;

        if (loc < startRotation) {
            times = (startRotation - loc) / delta;
        } else if (loc > endRotation) {
            times = (endRotation - loc) / delta;
        }

        if (times < 0) {
            times = floor(times);
        } else if (times > 0) {
            times = ceil(times);
        }

        loc += delta * times;

        if (extendMode == EXTEND_MODE_MIRROR) {
            bool shouldMirror = abs(times) % 2.0f != 0.0f;

            if (shouldMirror) {
                loc = endRotation - loc + startRotation;
            }
        }
    } else {
        discard;
        return float4(1.0f, 0.0f, 0.0f, 1.0f); // errored
    }

    float4 color1 = clamp(g_gradientStopColors[0], 0.0f, 1.0f);
    float4 color2 = clamp(g_gradientStopColors[1], 0.0f, 1.0f);
    float pos1 = startRotation + g_gradientStopPositions[0] * grad;
    float pos2 = startRotation + g_gradientStopPositions[1] * grad;

    float smooth = smooth = smoothstep(pos1, pos2, loc);
    float4 output = lerp(color1, color2, smooth);

    for (int i = 1; i < g_numGradientStops - 1; ++i) {
        color2 = clamp(g_gradientStopColors[i + 1], 0, 1);
        pos1 = startRotation + g_gradientStopPositions[i] * grad;
        pos2 = startRotation + g_gradientStopPositions[i + 1] * grad;

        smooth = smoothstep(pos1, pos2, loc);
        output = lerp(output, color2, smooth);
    }

    output.a *= g_opacity;

    return output;
}

float4 ps_srgb_clamp(in PS_IN input) : SV_TARGET {
    return ps(input, GAMMA_SRGB, EXTEND_MODE_CLAMP);
}

float4 ps_linear_clamp(in PS_IN input) : SV_TARGET {
    return ps(input, GAMMA_LINEAR, EXTEND_MODE_CLAMP);
}

float4 ps_srgb_wrap(in PS_IN input) : SV_TARGET {
    return ps(input, GAMMA_SRGB, EXTEND_MODE_WRAP);
}

float4 ps_linear_wrap(in PS_IN input) : SV_TARGET {
    return ps(input, GAMMA_LINEAR, EXTEND_MODE_WRAP);
}

float4 ps_srgb_mirror(in PS_IN input) : SV_TARGET {
    return ps(input, GAMMA_SRGB, EXTEND_MODE_MIRROR);
}

float4 ps_linear_mirror(in PS_IN input) : SV_TARGET {
    return ps(input, GAMMA_LINEAR, EXTEND_MODE_MIRROR);
}

technique LinearGradientBrush {
    pass SRgb_Clamp {
		VertexShader = compile VS_SHADERMODEL vs();
		PixelShader = compile PS_SHADERMODEL ps_srgb_clamp();
    }
    pass Linear_Clamp {
		VertexShader = compile VS_SHADERMODEL vs();
		PixelShader = compile PS_SHADERMODEL ps_linear_clamp();
    }
    pass SRgb_Wrap {
		VertexShader = compile VS_SHADERMODEL vs();
		PixelShader = compile PS_SHADERMODEL ps_srgb_wrap();
    }
    pass Linear_Wrap {
		VertexShader = compile VS_SHADERMODEL vs();
		PixelShader = compile PS_SHADERMODEL ps_linear_wrap();
    }
    pass SRgb_Mirror {
		VertexShader = compile VS_SHADERMODEL vs();
		PixelShader = compile PS_SHADERMODEL ps_srgb_mirror();
    }
    pass Linear_Mirror {
		VertexShader = compile VS_SHADERMODEL vs();
		PixelShader = compile PS_SHADERMODEL ps_linear_mirror();
    }
};
