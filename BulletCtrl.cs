using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BulletCtrl : NetworkBehaviour
{
	public LayerMask hitMask;
	NetworkObject Sender;
	float areaRadius;
	float areaImpulse;
	float areaDamage;
	float Damage;
	float bltSpeed;
	float Grafity;
	bool trueDmg;

	TickTimer timeToLife;
	TickTimer timeToFade;
	int vfxIdx;
	bool destroyed;

	[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
	public void RPC_bltView(int idx, RpcInfo info = default)
	{
		transform.GetChild(0).GetChild(idx).gameObject.SetActive(true);
		SoundManager.sm.weaponPlay(idx);
	}

    public void bulletInit(NetworkObject sender, float radius, float impulse, float areadmg, float dmg, float speed, float gravity, float life, float fade, int idx, bool truedmg)
	{
		Sender = sender;
		vfxIdx = idx;
		areaRadius = radius;
		areaImpulse = impulse;
		Damage = dmg;
		areaDamage = areadmg;
		bltSpeed = speed;
		Grafity = gravity;

		trueDmg = truedmg;

		timeToLife = TickTimer.CreateFromSeconds(Runner, life);
		timeToFade = TickTimer.CreateFromSeconds(Runner, fade);

		RPC_bltView(vfxIdx);
	}

	private void MoveBullet()
	{
		transform.position += (bltSpeed + Sender.transform.GetChild(0).GetComponent<charController>().playerSpeed * 1.6f) * transform.forward * Runner.DeltaTime;
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, areaRadius, hitMask);

		if (targetsInViewRadius.Length <= 0 || targetsInViewRadius[0].gameObject == Sender.transform.GetChild(0).gameObject)
			return;

		if (targetsInViewRadius[0].CompareTag("Player") || targetsInViewRadius[0].CompareTag("Villain"))
		{
			SoundManager.sm.impactPlay(vfxIdx);
			float damage = Damage;
			targetsInViewRadius[0].GetComponent<ICanTakeDamage>().takeDamage(Sender, damage, trueDmg);
		}

		destroyed = true;
	}

    public override void FixedUpdateNetwork()
	{
		if (!gameObject.GetComponent<NetworkObject>().HasInputAuthority)
			return;

		if (timeToLife.Expired(Runner) || destroyed)
        {
			destroy();
		}
		else
        {
			MoveBullet();
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
