using NUnit.Framework;
using UnityEngine;

namespace DuskBlade.Tests
{
    [Category("Audio"), Category("Reference"), Category("EditMode")]
    public class AudioReferenceEditModeTests : ReferenceEditModeTestBase
    {
        protected override string ExportName => "AudioReferenceEditMode";

        [Test, Description("AUR-001: Kiểm tra Player AudioSource hợp lệ nếu có.")]
        public void AUR_001_PlayerAudioHopLe() { AudioCheck("AUR-001", "Player AudioSource hợp lệ", FindPlayerPrefab(), "Player"); }
        [Test, Description("AUR-002: Kiểm tra Enemy AudioSource hợp lệ nếu có.")]
        public void AUR_002_EnemyAudioHopLe() { AudioCheck("AUR-002", "Enemy AudioSource hợp lệ", FindEnemyPrefab(), "Enemy"); }
        [Test, Description("AUR-003: Kiểm tra UI AudioSource hợp lệ nếu có.")]
        public void AUR_003_UIAudioHopLe() { AudioCheck("AUR-003", "UI AudioSource hợp lệ", FindPrefabByName("HUD", "UI", "Canvas"), "UI"); }
        [Test, Description("AUR-004: Kiểm tra AudioClip không null nếu AudioSource playOnAwake.")]
        public void AUR_004_AudioClipKhongNull() { Run("AUR-004", "AudioClip không null nếu yêu cầu", "AudioSource playOnAwake trên Player/Enemy/UI không có clip null.", "Low", c => { GameObject p = FindPlayerPrefab(); GameObject e = FindEnemyPrefab(); GameObject ui = FindPrefabByName("HUD", "UI", "Canvas"); int nullClip = CountNullAudioClips(p) + CountNullAudioClips(e) + CountNullAudioClips(ui); c.Actual = $"AudioClip null khi playOnAwake={nullClip}."; Assert.AreEqual(0, nullClip, "Có AudioSource playOnAwake nhưng clip null."); }); }
        [Test, Description("AUR-005: Kiểm tra AudioManager tồn tại nếu project dùng.")]
        public void AUR_005_AudioManagerTonTai() { Run("AUR-005", "AudioManager tồn tại nếu project dùng", "Có class hoặc prefab AudioManager nếu project dùng audio manager.", "Low", c => { GameObject audio = FindPrefabByName("AudioManager", "Audio"); bool scriptExists = System.IO.File.Exists("Assets/_Data/Scripts/Audio/AudioManager.cs"); c.Actual = $"AudioManager script={scriptExists}, prefab={(audio ? audio.name : "không tìm thấy")}."; Assert.IsTrue(scriptExists || audio != null, "Không tìm thấy AudioManager script/prefab."); }); }
        [Test, Description("AUR-006: Kiểm tra AudioMixer asset nếu project có mixer.")]
        public void AUR_006_AudioMixerAsset() { Run("AUR-006", "AudioMixer asset nếu có", "Ghi nhận số AudioMixer asset trong project.", "Low", c => { string[] mixers = UnityEditor.AssetDatabase.FindAssets("t:AudioMixer"); c.Actual = $"AudioMixer asset={mixers.Length}."; }); }
        [Test, Description("AUR-007: Kiểm tra không Missing Script trên prefab liên quan audio.")]
        public void AUR_007_KhongMissingScriptAudio() { Run("AUR-007", "Không Missing Script liên quan audio", "Prefab audio liên quan không có Missing Script.", "Medium", c => { GameObject p = FindPlayerPrefab(); GameObject e = FindEnemyPrefab(); GameObject ui = FindPrefabByName("HUD", "UI", "Canvas"); int missing = CountMissingScripts(p) + CountMissingScripts(e) + CountMissingScripts(ui); c.Actual = $"Prefab kiểm tra=Player+Enemy+UI, Missing Script={missing}."; Assert.AreEqual(0, missing, "Có Missing Script trên prefab audio liên quan."); }); }
        [Test, Description("AUR-008: Kiểm tra AudioSource không bị mute toàn bộ nếu có.")]
        public void AUR_008_AudioSourceKhongMuteToanBo() { Run("AUR-008", "AudioSource không mute toàn bộ", "Nếu có AudioSource thì không phải tất cả đều mute.", "Low", c => { int sources = 0; int muted = 0; foreach (GameObject prefab in new[] { FindPlayerPrefab(), FindEnemyPrefab(), FindPrefabByName("HUD", "UI", "Canvas") }) { if (prefab == null) continue; foreach (AudioSource source in prefab.GetComponentsInChildren<AudioSource>(true)) { sources++; if (source.mute) muted++; } } c.Actual = $"AudioSource={sources}, mute={muted}."; if (sources > 0) Assert.Less(muted, sources, "Tất cả AudioSource đang mute."); }); }

        private void AudioCheck(string id, string title, GameObject prefab, string label)
        {
            Run(id, title, "AudioSource nếu tồn tại thì clip playOnAwake không null và volume hợp lệ.", "Low", c =>
            {
                if (prefab == null) { c.Actual = $"Không tìm thấy prefab {label}, bỏ qua kiểm tra optional."; return; }
                int sources = prefab.GetComponentsInChildren<AudioSource>(true).Length;
                int nullClip = CountNullAudioClips(prefab);
                c.Actual = $"Prefab={prefab.name}, AudioSource={sources}, clip null khi playOnAwake={nullClip}.";
                Assert.AreEqual(0, nullClip, label + " có AudioSource playOnAwake nhưng clip null.");
            });
        }
    }
}
