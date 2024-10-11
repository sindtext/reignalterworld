using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TrowCtrl : NetworkBehaviour
{
	public LayerMask hitMask;
	NetworkObject Sender;
	float areaRadius;
	float areaImpulse;
	float Damage;
	float areaDamage;
	float bltSpeed;
	float Grafity;
	bool trueDmg;
	Rigidbody rb;

	TickTimer timeToLife;
	TickTimer timeToFade;
	int vfxIdx;
	bool destroyed;

	private void Start()
	{

	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
	public void RPC_bltView(int idx, RpcInfo info = default)
	{
		transform.GetChild(0).GetChild(idx).gameObject.SetActive(true);
		SoundManager.sm.weaponPlay(idx);
	}

	public void bulletInit(NetworkObject sender, float radius, float impulse, float areadmg, float dmg, float speed, float grafity, float life, float fade, int idx, bool truedmg)
	{
		Sender = sender;
		vfxIdx = idx;
		areaRadius = radius;
		areaImpulse = impulse;
		Damage = dmg;
		areaDamage = areadmg;
		bltSpeed = speed;
		Grafity = grafity;

		trueDmg = truedmg;

		timeToLife = TickTimer.CreateFromSeconds(Runner, life);
		timeToFade = TickTimer.CreateFromSeconds(Runner, fade);
		transform.GetChild(0).GetChild(vfxIdx).gameObject.SetActive(true);

		transform.parent = null;

		rb = gameObject.GetComponent<Rigidbody>();
		Trowing();
	}

	private void Trowing()
	{
		Vector3 trowAngel = (transform.up * Grafity).normalized;
		Vector3 trowDirection = (transform.forward + trowAngel).normalized;
		rb.AddForce(trowDirection * bltSpeed, ForceMode.VelocityChange);
	}

	public override void FixedUpdateNetwork()
	{
		if(!gameObject.GetComponent<NetworkObject>().HasInputAuthority)
			return;

		if (timeToLife.Expired(Runner))
		{
			Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, areaRadius, hitMask);

			foreach (Collider col in targetsInViewRadius)
			{
				if (col.CompareTag("Player") || col.CompareTag("Villain"))
				{
					float damage = Damage - areaDamage * Vector3.Distance(col.transform.position, transform.position);
					col.GetComponent<ICanTakeDamage>().takeDamage(Sender, damage, trueDmg);
				}
			}

			RPC_bltView(vfxIdx);
			Invoke("destroy", 1);
		}
	}

    private void destroy()
	{
		Runner.Despawn(gameObject.GetComponent<NetworkObject>());
	}

    private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, areaRadius);
	}
}
