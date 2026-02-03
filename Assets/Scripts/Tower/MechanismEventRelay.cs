using UnityEngine;

public class MechanismEventRelay : MonoBehaviour
{
    [SerializeField] private StoneTower tower;

    public void OnMechanismRaised()
    {
        if (tower != null)
            tower.OnMechanismRaised();
    }

    public void OnMechanismAnimationComplete()
    {
        if (tower != null)
            tower.OnMechanismAnimationComplete();
    }
}
