using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Mockup of a real, superior Forniture class*/

public abstract class FornitureController : MonoBehaviour {
    public abstract float GetUnitDim();
    public abstract void Build();
    public abstract void Demolish();
    public abstract void UpdateDimensions(Vector2 Axis, Vector3 Direction);
}
