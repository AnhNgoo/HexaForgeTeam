# Walkthrough: 3D to 2D Asset Capture Tool

## Tổng Kết

Tool **"3D to 2D Asset Capture"** đã được tạo thành công tại:
`Assets/Editors/AssetCapture/`

---

## Files Đã Tạo

| File | Dòng | Mô tả |
|------|------|-------|
| `AssetCapturePreset.cs` | 189 | ScriptableObject lưu toàn bộ cấu hình |
| `AssetCaptureUtility.cs` | 233 | Static helpers: bounds, path, IMGUI |
| `AssetCaptureCamera.cs` | 202 | Camera orbit/pan/zoom controller |
| `AssetCaptureLighting.cs` | 104 | 3-light system trong preview scene |
| `AssetCaptureRenderer.cs` | 324 | Core render engine (Preview Scene + RT) |
| `AssetCaptureImageAdjust.cs` | 133 | Image adjustment via Graphics.Blit |
| `AssetCaptureExporter.cs` | 178 | PNG/JPG export + TextureImporter auto-setup |
| `AssetCaptureBatchExporter.cs` | 171 | Batch export UI + logic |
| `AssetCaptureWindow.cs` | 750 | Main EditorWindow (toàn bộ IMGUI layout) |
| `Shaders/ImageAdjust.shader` | 73 | CG shader brightness/contrast/sat/exp/gamma |
| `README.md` | 152 | Hướng dẫn sử dụng |

**Tổng: ~2,509 dòng code**

---

## Cách Hoạt Động

### Rendering Pipeline (URP Compatible)

```
Drag Prefab
    ↓
EditorSceneManager.NewPreviewScene()   ← isolated scene
    ↓
Instantiate prefab → MoveGameObjectToScene(previewScene)
    ↓
Camera + 3 Lights → MoveGameObjectToScene(previewScene)
    ↓
camera.Render() → RenderTexture ARGB32    ← URP handles this
    ↓
Graphics.Blit() + ImageAdjust.shader      ← post-process
    ↓
GUI.DrawTexture(previewRT)                ← preview display
    ↓
On Export: ReadPixels() → EncodeToPNG/JPG → File.WriteAllBytes()
    ↓
AssetDatabase.Refresh() → TextureImporter (Sprite 2D) auto-applied
```

### Cleanup

Khi tool đóng (`OnDisable()`):
- `EditorSceneManager.ClosePreviewScene()` → xóa toàn bộ preview scene
- `RenderTexture.Release()` → giải phóng VRAM
- `DestroyImmediate()` → không để lại GameObject trong hierarchy

---

## Tính Năng Hoàn Thành

- [x] Menu `Tools → 3D to 2D Asset Capture`
- [x] EditorWindow với layout 2-panel (left controls + right preview)
- [x] Drag & Drop prefab
- [x] Real-time preview (cập nhật khi thay đổi bất kỳ setting)
- [x] Camera Orbit/Pan/Zoom bằng mouse trong preview rect
- [x] Orthographic + Perspective mode
- [x] Camera position/rotation/distance/FOV/OrthoSize controls
- [x] Object Transform (position/rotation/scale) + Reset
- [x] Auto Center + Fit To Frame (tính Bounds tổng renderer)
- [x] Transparent Background (ARGB32 RT, alpha=0 clear)
- [x] Solid Color Background
- [x] 3-Light system: Main/Fill/Rim (enable/disable, rotation, intensity, color)
- [x] Ambient color + intensity
- [x] Image Adjustment: Brightness/Contrast/Saturation/Exposure/Gamma
- [x] Real-time slider updates
- [x] Export PNG (với alpha) / JPG
- [x] Resolution presets: 128/256/512/1024/2048 + Custom W×H
- [x] Browse path picker
- [x] Overwrite toggle
- [x] Auto TextureImporter settings (Sprite 2D, SpriteMode, Compression)
- [x] Save/Load Preset (ScriptableObject .asset)
- [x] Batch Export (danh sách prefab + Export All)
- [x] Consistent icon scale qua Bounds-based framing + padding
- [x] Checkerboard preview background (hiển thị transparency)
- [x] Resizable preview panel (drag handle)
- [x] Scene isolation (không ảnh hưởng Main Camera / Scene Lighting)
- [x] URP compatible (reflection-based `UniversalAdditionalCameraData`)
- [x] Full cleanup on close

---

## Giới Hạn Biết Trước (Unity 2022.3 + URP)

| Giới hạn | Lý do | Giải pháp áp dụng |
|----------|-------|-------------------|
| ParticleSystem preview | Cần simulate timeline đầy đủ | Render frame hiện tại |
| Shadow trong preview | Preview scene không có shadow caster setup | Shadow disabled, dùng 3-light rig |
| URP Post-processing stack | Không apply URP Volume effects trong preview scene | Image adjustment qua shader riêng |
| Real-time reflection probes | Không khả thi trong preview scene | Không hỗ trợ |
