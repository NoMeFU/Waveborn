using UnityEngine;

public class AnimatorTest : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        Debug.Log($"Animator знайдено: {anim != null}");
    }

    void Update()
    {
        if (!anim) return;

        float forward = 0f;
        float right = 0f;
        float speed = 0f;

        if (Input.GetKey(KeyCode.W)) forward = 1f;
        if (Input.GetKey(KeyCode.S)) forward = -1f;
        if (Input.GetKey(KeyCode.D)) right = 1f;
        if (Input.GetKey(KeyCode.A)) right = -1f;

        speed = Mathf.Sqrt(forward * forward + right * right) * 7f;

        anim.SetFloat("Speed", speed);
        anim.SetFloat("Forward", forward);
        anim.SetFloat("Right", right);

        Debug.Log($"Speed={speed:F2}, Forward={forward:F2}, Right={right:F2}");
    }

    void OnGUI()
    {
        if (anim)
        {
            GUI.Label(new Rect(10, 10, 400, 20), $"Speed: {anim.GetFloat("Speed"):F2}");
            GUI.Label(new Rect(10, 30, 400, 20), $"Forward: {anim.GetFloat("Forward"):F2}");
            GUI.Label(new Rect(10, 50, 400, 20), $"Right: {anim.GetFloat("Right"):F2}");
        }
    }
}