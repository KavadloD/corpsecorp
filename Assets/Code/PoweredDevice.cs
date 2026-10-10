using UnityEngine;

public abstract class PoweredDevice : MonoBehaviour
{
    public bool requiresPower = true;

    bool powered;

    public bool IsOn => !requiresPower || powered;

    public void SetPowered(bool on)
    {
        powered = on;
    }
}