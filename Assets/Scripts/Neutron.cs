using UnityEngine;

public class Neutron : MonoBehaviour
{
    public int? Id { get; set; } = null;
    public bool IsBeingAnimated { get; set; } = false;
    public bool Selected { get; set; } = false;
    public int ColorId { get; set; } = 0;
}