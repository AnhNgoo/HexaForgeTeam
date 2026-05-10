using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeSwitcher : Singleton<VolumeSwitcher>
{
    [SerializeField] private Volume volume;
    public Volume Volume => volume;
    [SerializeField] private VolumeProfile defaultVolumeProfile;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (volume == null)
            volume = GetComponent<Volume>();

        if (defaultVolumeProfile == null)
            defaultVolumeProfile = Resources.Load<VolumeProfile>("Volumes/DuskBlade");
    }

    private void Start()
    {
        ResetToDefaultProfile();
    }
    public void ChangeVolumeProfile(VolumeProfile newProfile)
    {
        if (volume == null || newProfile == null) return;

        volume.profile = newProfile;
    }

    public void ResetToDefaultProfile()
    {
        if (volume == null || defaultVolumeProfile == null) return;

        volume.profile = defaultVolumeProfile;
    }
}
