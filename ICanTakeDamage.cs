using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public interface ICanTakeDamage
{
	public void takeDamage(NetworkObject sender, float damage, bool trueDmg);
	public bool areDeath();
}