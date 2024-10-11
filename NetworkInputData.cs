using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public const byte btnAct = 0x01;
    public const byte btnSkillA = 0x02;
    public const byte btnSkillB = 0x03;

    public byte pushBtn;
}

