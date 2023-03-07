@FOR %%a IN (*.fx) DO @(
    mgfxc "%%a" "%%a.ogl.mgfxo" /Profile:OpenGL
    mgfxc "%%a" "%%a.dx11.mgfxo" /Profile:DirectX_11
)
