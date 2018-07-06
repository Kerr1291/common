using UnityEngine;
using System.Collections;

public class PlayerGameInterface : MonoBehaviour
{
    [SerializeField]
    private PlayerInputInterface owner;

    public Avatar inputTarget = null;        
    public void SetTarget( Avatar avatar )
    {
        inputTarget = avatar;
    }

    public Avatar GetTarget()
    {
        return inputTarget;
    }
}
