using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light flashlight;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private float minIntensity = 0.2f;
    [ReadOnly] [SerializeField] private float changeRate;
    [SerializeField] private float sprintRate = -0.2f;
    [SerializeField] private float normalRate = 0.1f;

    private void Update()
    {
        flashlight.intensity += changeRate * Time.deltaTime;
        flashlight.intensity = Mathf.Clamp(flashlight.intensity, minIntensity, maxIntensity);
    }

    public void SetSprintRate()
    {
        changeRate = sprintRate;
    }

    public void SetNormalRate()
    {
        changeRate = normalRate;
    }

    public void EnableFlashlight()
    {
        flashlight.enabled = true;
    }

    public void DisableFlashlight()
    {
        flashlight.intensity = minIntensity;
        flashlight.enabled = false;
    }
}
