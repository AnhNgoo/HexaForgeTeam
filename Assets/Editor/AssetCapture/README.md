# 3D to 2D Asset Capture – Hướng Dẫn Sử Dụng
## HexaForge | Unity 2022.3 + URP

---

## 1. CẤU TRÚC FILE

```
Assets/Editors/AssetCapture/
├── AssetCaptureWindow.cs         ← EditorWindow chính (menu entry)
├── AssetCaptureRenderer.cs       ← Core render engine (Preview Scene)
├── AssetCaptureCamera.cs         ← Camera orbit/pan/zoom controller
├── AssetCaptureLighting.cs       ← 3-light system (Main/Fill/Rim)
├── AssetCaptureExporter.cs       ← Export PNG/JPG + TextureImporter
├── AssetCapturePreset.cs         ← ScriptableObject data model
├── AssetCaptureUtility.cs        ← Static helpers
├── AssetCaptureBatchExporter.cs  ← Batch export UI + logic
├── AssetCaptureImageAdjust.cs    ← Image adjustment Material manager
└── Shaders/
    └── ImageAdjust.shader        ← Brightness/Contrast/Saturation shader
```

---

## 2. MỞ TOOL

Vào menu Unity:

```
Tools → 3D to 2D Asset Capture
```

Cửa sổ EditorWindow sẽ mở ra. Kéo rộng để có không gian làm việc tốt nhất.

---

## 3. EXPORT MỘT PREFAB

**Bước 1:** Kéo prefab vào ô **Prefab / Model** (drag & drop, hoặc dùng picker).

**Bước 2:** Kiểm tra Preview ở bên phải. Dùng chuột để điều chỉnh góc nhìn:
- **Left drag** → Xoay camera (Orbit)
- **Middle drag / Alt+Right** → Pan camera  
- **Scroll wheel** → Zoom in/out

**Bước 3:** Chỉnh cấu hình trong các section bên trái:

| Section | Tác dụng |
|---------|----------|
| Camera | Orthographic/Perspective, FOV, khoảng cách |
| Object Transform | Vị trí/xoay/scale của model |
| Auto Framing | Tự căn giữa và fit object vào frame |
| Background | Transparent hoặc màu nền |
| Lighting | 3 directional lights + Ambient |
| Image Adjustment | Brightness, Contrast, Saturation, Exposure, Gamma |

**Bước 4:** Trong section **Export**:
- Chọn Resolution (mặc định 512×512)
- Nhập đường dẫn xuất (mặc định `Assets/2DAssetItem`)
- Nhập tên file (mặc định là tên prefab)

**Bước 5:** Nhấn **EXPORT PNG** hoặc **EXPORT JPG**.

File sẽ được lưu, tự động import vào AssetDatabase với TextureType = Sprite.

---

## 4. TẠO VÀ SỬ DỤNG PRESET

### Lưu Preset

1. Chỉnh tất cả cấu hình muốn (camera, lighting, background, image adjust...)
2. Cuộn xuống section **Preset**
3. Nhấn **New Preset Asset** → file `.asset` được tạo tại `Assets/Editors/AssetCapture/Presets/`
4. Hoặc nhấn **Save Preset** nếu đã có preset đang chọn → ghi đè

### Load Preset

1. Kéo file `.asset` preset vào ô **Preset Asset**
2. Nhấn **Load Preset** → toàn bộ cấu hình được áp dụng

### Workflow nhiều weapon với cùng style

```
1. Chỉnh cấu hình một lần cho Sword → Save Preset "WeaponIconPreset"
2. Drag Axe.prefab → Load Preset "WeaponIconPreset" → Export
3. Drag Hammer.prefab → Load Preset "WeaponIconPreset" → Export
4. Drag Bow.prefab → Load Preset "WeaponIconPreset" → Export
```

Tất cả icon sẽ có cùng góc nhìn, lighting, và visual scale.

---

## 5. BATCH EXPORT

1. Cuộn xuống section **Batch Export**
2. Kéo nhiều prefab vào list, hoặc nhấn **+ Add Slot** rồi pick từng cái
3. Đảm bảo đã chọn đúng preset hoặc đã chỉnh cấu hình
4. Nhấn **EXPORT ALL** → tool sẽ render và export từng prefab

Tên file = tên prefab. Tất cả file xuất vào cùng một folder.

---

## 6. GIẢI THÍCH KỸ THUẬT

### Preview Scene (không ảnh hưởng Scene thực)

Tool dùng `EditorSceneManager.NewPreviewScene()` để tạo một scene cô lập.
Tất cả object (camera, lights, prefab instance) đều ở trong scene đó.
Scene bị đóng khi tool đóng. Không để lại rác trong Hierarchy.

### URP Transparent Background

- RenderTexture format: `ARGB32`
- Camera clear color: `rgba(0,0,0,0)`
- URP sẽ giữ alpha=0 ở vùng không có geometry
- PNG giữ nguyên alpha channel

### Image Adjustment

- Shader `Hidden/HexaForge/ImageAdjust` xử lý qua `Graphics.Blit()`
- KHÔNG thay đổi material gốc của prefab
- Chỉ apply vào render output

---

## 7. GIỚI HẠN BIẾT TRƯỚC

| Vấn đề | Giải thích |
|--------|-----------|
| ParticleSystem | Chỉ render frame hiện tại, không simulate animation |
| Transparent với một số URP shader | Shader tự viết alpha=0 có thể ghi đè, nhưng URP Lit chuẩn hoạt động tốt |
| Preview không phải Game View | Preview dùng isolated scene, không giống 100% in-game nếu có post-processing stack trong game scene |
| Shadow Softness | URP 2022.3 không expose trực tiếp qua editor API, shadow disabled trong preview để tránh artifacts |

---

## 8. TROUBLESHOOTING

**Preview không hiển thị?**
→ Đảm bảo URP pipeline asset đã được cấu hình. Tool thêm `UniversalAdditionalCameraData` tự động qua reflection.

**Export bị đen / không có transparency?**
→ Kiểm tra: Background section → bật **Transparent Background**. Format phải là PNG (JPG không có alpha).

**Shader không tìm thấy (Image Adjustment không hoạt động)?**
→ Đảm bảo file `Shaders/ImageAdjust.shader` tồn tại và Unity đã import nó (Refresh AssetDatabase).

**Preset không load?**
→ Đảm bảo preset `.asset` được chọn trong ô "Preset Asset" trước khi nhấn Load Preset.
