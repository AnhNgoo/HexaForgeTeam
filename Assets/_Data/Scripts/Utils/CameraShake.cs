using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Cinemachine;

public class CameraShake : Singleton<CameraShake>
{
	[SerializeField] private CinemachineImpulseSource impulseSource;
	public void Shake()
	{
		impulseSource.GenerateImpulse();
	}
}