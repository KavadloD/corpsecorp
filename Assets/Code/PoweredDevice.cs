using UnityEngine;

public abstract class PoweredDevice : MonoBehaviour
{
    public bool requiresPower = true;

    int powerCount;

    public bool IsOn => !requiresPower || powerCount > 0;

    public void SetPowered(bool on)
    {
        powerCount = Mathf.Max(0, powerCount + (on ? 1 : -1));
    }
}