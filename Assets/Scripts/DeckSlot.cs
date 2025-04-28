using System;
using UnityEngine;

[Serializable]
public class DeckSlot
{
    public UnitData unit;   // ton ScriptableObject, cf. UnitData.cs :contentReference[oaicite:0]{index=0}&#8203;:contentReference[oaicite:1]{index=1}
    public Sex sex;     // Male ou Female
}

