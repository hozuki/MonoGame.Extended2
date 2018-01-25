#define MAX_GRADIENT_STOPS (16)

struct GradientStop {
    float Position;
    float4 Color;
};

cbuffer cbGradient {
    GradientStop g_gradientStops[MAX_GRADIENT_STOPS];
    int g_numGradientStop;
    int g_gamma;
    int g_extendMode;
}
