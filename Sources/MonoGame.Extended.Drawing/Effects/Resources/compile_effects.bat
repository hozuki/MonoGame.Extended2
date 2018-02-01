@FOR %%a IN (*.fx) DO @(
    "C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe" "%%a" "%%a.ogl.mgfxo" /Profile:OpenGL
    "C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe" "%%a" "%%a.dx11.mgfxo" /Profile:DirectX_11
)
