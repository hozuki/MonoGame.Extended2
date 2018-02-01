#define MAX_GRADIENT_STOPS (32)

cbuffer cbGradient {
    float g_gradientStopPositions[MAX_GRADIENT_STOPS];
    float4 g_gradientStopColors[MAX_GRADIENT_STOPS];
    int g_numGradientStops;
}

#define GRADIENT_PARAMS_DECL uniform int gamma, uniform int extendMode

#define GAMMA_SRGB (0)
#define GAMMA_LINEAR (1)
#define EXTEND_MODE_CLAMP (0)
#define EXTEND_MODE_WRAP (1)
#define EXTEND_MODE_MIRROR (2)
