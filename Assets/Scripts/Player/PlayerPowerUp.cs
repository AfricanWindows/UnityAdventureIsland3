using UnityEngine;

public class PlayerPowerUp : MonoBehaviour
{
    public void CollectPowerUp(IPowerUp powerUp)
    {
        powerUp.ApplyPowerUp(this.gameObject);
    }
}
