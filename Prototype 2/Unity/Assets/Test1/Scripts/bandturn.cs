using UnityEngine;
using Oculus.Interaction.Locomotion;

public class DisableTurnsAtRuntime : MonoBehaviour
{
    void Start()
    {
        // 禁用所有转向可视化
        foreach (var v in FindObjectsOfType<TurnArrowVisuals>(true))
        {
            v.enabled = false;
            v.gameObject.SetActive(false);
        }

        // 禁用所有转向交互器
        foreach (var t in FindObjectsOfType<LocomotionAxisTurnerInteractor>(true))
        {
            t.enabled = false;
            t.gameObject.SetActive(false);
        }
        foreach (var t in FindObjectsOfType<AnimatedSnapTurnVisuals>(true))
        {
            t.enabled = false;
            t.gameObject.SetActive(false);
        }
    }
}
