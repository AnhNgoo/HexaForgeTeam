using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Cinemachine;

public class CameraShake : Singleton<CameraShake>
{
	[SerializeField] private CinemachineImpulseSource impulseSource;
	private bool isShaking = false;

	public void SetImpulseSource(CinemachineImpulseSource source)
	{
		impulseSource = source;
	}
	public async void Shake()
	{
		if (impulseSource == null)
			return;

		if (!isShaking)
		{
			isShaking = true;
			impulseSource.GenerateImpulse();
			await System.Threading.Tasks.Task.Delay(500); // Wait for 0.5 seconds
			isShaking = false;
		}
	}
}