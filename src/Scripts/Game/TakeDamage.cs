using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EDamageType
{
	Generic,
	Environment,
	Slash,
	Fire,
	Ice,
	Electric
};

// A simple interface used for dispensing damage to enemies, characters, and world props
// Just provide a function definition for OnTakeDamage and you're ready to rock!
public interface ITakeDamage
{
	void OnTakeDamage(GameObject attacker, float dmgAmount, EDamageType dmgType = EDamageType.Generic);
}

